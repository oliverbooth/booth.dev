using System.Text.RegularExpressions;
using System.Web;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;
using BoothDotDev.Markdown;
using BoothDotDev.Markdown.CodeBlock;
using BoothDotDev.Markdown.Embed;
using BoothDotDev.Markdown.Link;
using Humanizer;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Options;
using MD = Markdig.Markdown;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for rendering Markdown content into HTML.
/// </summary>
public sealed class MarkdownRenderingService
{
    // Block math (\begin{...}...\end{...}) first, so it isn't mistaken for a run of inline math; $$...$$ before
    // $...$ for the same reason.
    private static readonly Regex LatexPattern = new(
        @"\\begin\{[a-zA-Z*]+\}.*?\\end\{[a-zA-Z*]+\}|\$\$.*?\$\$|\\\[.*?\\\]|\\\(.*?\\\)|\$[^$\n]+\$",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex WhitespaceRunPattern = new(@"\s+", RegexOptions.Compiled);

    private readonly MarkdownPipeline _markdownPipeline;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MarkdownRenderingService" /> class.
    /// </summary>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" /> to use for rendering Markdown.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    public MarkdownRenderingService(MarkdownPipeline markdownPipeline, IServiceScopeFactory serviceScopeFactory)
    {
        _markdownPipeline = markdownPipeline;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    ///     Renders the specified Markdown content into HTML using the configured Markdown pipeline.
    /// </summary>
    /// <param name="body">The Markdown content to render.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="published">The published date and time.</param>
    /// <param name="area">The area.</param>
    /// <returns>The HTML content of the rendered content.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="body" /> is <see langword="null" />.</exception>
    public string Render(string body, Guid id, DateTimeOffset published, string? area = null)
    {
        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        using var writer = new StringWriter();
        using var scope = _serviceScopeFactory.CreateScope();

        HtmlRenderer htmlRenderer = CreateHtmlRenderer(writer, area ?? "content", id, published, scope.ServiceProvider);

        var document = MD.Parse(body, _markdownPipeline);
        htmlRenderer.Render(document);
        writer.Flush();

        return writer.ToString();
    }

    /// <summary>
    ///     Renders the body of a <see cref="IMarkdownBody" /> as HTML using the configured Markdown pipeline.
    /// </summary>
    /// <param name="body">The <see cref="IMarkdownBody" /> to render.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="published">The published date and time.</param>
    /// <returns>The HTML content of the rendered content.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="body" /> is <see langword="null" />.</exception>
    public string Render(IMarkdownBody body, Guid id, DateTimeOffset published)
    {
        var area = body switch
        {
            BlogPost => "blog",
            TutorialArticle => "tutorial",
            ProjectDevlog => "devlog",
            Note => "note",
            _ => "content"
        };

        return Render(body.Body, id, published, area);
    }

    /// <summary>
    ///     Renders the excerpt of the specified blog post.
    /// </summary>
    /// <param name="markdown">The blog post whose excerpt to render.</param>
    /// <param name="wasTrimmed">
    ///     When this method returns, contains <see langword="true" /> if the excerpt was trimmed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>The rendered HTML of the blog post's excerpt.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="markdown" /> is <see langword="null" />.</exception>
    public string RenderExcerpt(IMarkdownExcerpt markdown, out bool wasTrimmed)
    {
        if (markdown is null)
        {
            throw new ArgumentNullException(nameof(markdown));
        }

        if (!string.IsNullOrWhiteSpace(markdown.Excerpt))
        {
            wasTrimmed = false;
            return MD.ToHtml(markdown.Excerpt, _markdownPipeline);
        }

        var body = markdown.Body;
        var moreIndex = body.IndexOf("<!--more-->", StringComparison.Ordinal);

        if (moreIndex == -1)
        {
            var excerpt = body.Truncate(255, "...");
            wasTrimmed = body.Length > 255;
            return MD.ToHtml(excerpt, _markdownPipeline);
        }

        wasTrimmed = true;
        return MD.ToHtml(body[..moreIndex], _markdownPipeline);
    }

    /// <summary>
    ///     Renders the plain text excerpt of the specified article.
    /// </summary>
    /// <param name="markdown">The article whose excerpt to render.</param>
    /// <param name="wasTrimmed">
    ///     When this method returns, contains <see langword="true" /> if the excerpt was trimmed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>The rendered plain text of the article's excerpt.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="markdown" /> is <see langword="null" />.</exception>
    public string RenderPlainTextExcerpt(IMarkdownExcerpt markdown, out bool wasTrimmed)
    {
        if (markdown is null)
        {
            throw new ArgumentNullException(nameof(markdown));
        }

        if (!string.IsNullOrWhiteSpace(markdown.Excerpt))
        {
            wasTrimmed = false;
            return ToPlainText(StripLatex(markdown.Excerpt));
        }

        var body = StripLatex(markdown.Body);
        var moreIndex = body.IndexOf("<!--more-->", StringComparison.Ordinal);

        if (moreIndex == -1)
        {
            var excerpt = body.Truncate(255, "...");
            wasTrimmed = body.Length > 255;
            return ToPlainText(excerpt);
        }

        wasTrimmed = true;
        return ToPlainText(body[..moreIndex]);
    }

    /// <summary>
    ///     Renders a truncated plain-text preview of a raw Markdown string, for content types that don't implement
    ///     <see cref="IMarkdownExcerpt" /> (and so have no dedicated excerpt field or <c>&lt;!--more--&gt;</c> convention).
    /// </summary>
    /// <param name="markdown">The raw Markdown to preview.</param>
    /// <param name="maxLength">The maximum length, in characters, of the Markdown source to consider before rendering.</param>
    /// <returns>The rendered plain text of the truncated preview.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="markdown" /> is <see langword="null" />.</exception>
    public string RenderPlainTextPreview(string markdown, int maxLength = 255)
    {
        if (markdown is null)
        {
            throw new ArgumentNullException(nameof(markdown));
        }

        return ToPlainText(StripLatex(markdown).Truncate(maxLength, "...")).Trim();
    }

    /// <summary>
    ///     Renders a truncated HTML preview of a raw Markdown string, for content types that don't implement
    ///     <see cref="IMarkdownExcerpt" /> - the HTML-returning counterpart to <see cref="RenderPlainTextPreview" />,
    ///     for consumers that render the result (an RSS feed reader) rather than draw it as bare text.
    /// </summary>
    /// <param name="markdown">The raw Markdown to preview.</param>
    /// <param name="maxLength">The maximum length, in characters, of the Markdown source to consider before rendering.</param>
    /// <returns>The rendered HTML of the truncated preview.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="markdown" /> is <see langword="null" />.</exception>
    public string RenderHtmlPreview(string markdown, int maxLength = 255)
    {
        if (markdown is null)
        {
            throw new ArgumentNullException(nameof(markdown));
        }

        return MD.ToHtml(markdown.Truncate(maxLength, "..."), _markdownPipeline);
    }

    /// <summary>
    ///     Renders Markdown to plain text, then decodes the result.
    /// </summary>
    /// <param name="markdown">The Markdown to render.</param>
    /// <returns>The rendered plain text.</returns>
    /// <remarks>
    ///     The typographer extension renders curly quotes, ellipses, etc. as literal HTML entities (e.g. <c>&amp;ldquo;</c>),
    ///     which <see cref="MD.ToPlainText(string, MarkdownPipeline, MarkdownParserContext)" /> doesn't decode on its own since
    ///     it's meant to strip tags, not entities. Left undecoded, plain-text consumers (an image canvas, a further-HTML-encoded
    ///     meta tag) would show the literal entity text instead of the character it represents.
    /// </remarks>
    private string ToPlainText(string markdown)
    {
        var plainText = HttpUtility.HtmlDecode(MD.ToPlainText(markdown, _markdownPipeline));
        return WhitespaceRunPattern.Replace(plainText, " ").Trim();
    }

    /// <summary>
    ///     Strips LaTeX math markup from a raw Markdown string.
    /// </summary>
    /// <param name="markdown">The raw Markdown to strip.</param>
    /// <returns>The Markdown with LaTeX math markup removed.</returns>
    /// <remarks>
    ///     MathJax typesets this client-side on the live page, but plain-text consumers (an OG image, a further-HTML-encoded meta
    ///     tag) have no typesetting step of their own - left in, the raw source (<c>\begin{align}</c>, <c>\tag{1}</c>,
    ///     <c>\forall</c>, ...) leaks through verbatim instead of reading as prose.
    /// </remarks>
    private static string StripLatex(string markdown)
    {
        return LatexPattern.Replace(markdown, string.Empty);
    }

    /// <summary>
    ///     Renders the table of contents for the specified blog post.
    /// </summary>
    /// <param name="markdown">The blog post whose table of contents to render.</param>
    /// <param name="request">The HTTP request.</param>
    /// <returns>The rendered HTML of the blog post's table of contents.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="markdown" /> is <see langword="null" />.</exception>
    public string RenderTableOfContents(string markdown, HttpRequest? request)
    {
        if (markdown is null)
        {
            throw new ArgumentNullException(nameof(markdown));
        }

        List<TocItem> items = MarkdownTocBuilder.BuildToc(markdown);
        return MarkdownTocBuilder.RenderTocAsHtml(items, request);
    }

    /// <summary>
    ///     Finds every bare filename referenced as CDN media in a Markdown body - both <c>![alt](filename)</c> and
    ///     <c>![[filename]]</c> formats.
    /// </summary>
    /// <param name="body">The Markdown content to scan.</param>
    /// <returns>The distinct filenames referenced, in first-seen order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="body" /> is <see langword="null" />.</exception>
    /// <remarks>
    ///     This method excludes <c>.md</c>/<c>.html</c> embeds, which transclude a local document rather than resolving through
    ///     the CDN.
    /// </remarks>
    public IReadOnlyList<string> FindMediaReferences(string body)
    {
        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        MarkdownDocument document = MD.Parse(body, _markdownPipeline);
        var references = new List<string>();

        foreach (LinkInline link in document.Descendants<LinkInline>())
        {
            if (link.IsImage && !string.IsNullOrEmpty(link.Url))
            {
                references.Add(link.Url);
            }
        }

        foreach (EmbedInline embed in document.Descendants<EmbedInline>())
        {
            var extension = Path.GetExtension(embed.FileName).ToLowerInvariant();
            if (extension is not (".html" or ".md"))
            {
                references.Add(embed.FileName);
            }
        }

        return references.Distinct(StringComparer.Ordinal).ToList();
    }

    private HtmlRenderer CreateHtmlRenderer(TextWriter writer,
        string area,
        Guid id,
        DateTimeOffset published,
        IServiceProvider services)
    {
        var htmlRenderer = new HtmlRenderer(writer);
        _markdownPipeline.Setup(htmlRenderer);

        var context = new MarkdownRenderContext(id, published);
        var razorPartialRenderer = services.GetRequiredService<RazorPartialRenderer>();
        var cdnBaseUrl = services.GetRequiredService<IOptions<CdnOptions>>().Value.BaseUrl;
        var resolver = new CdnMediaResolver(context, razorPartialRenderer, area, cdnBaseUrl);

        ReplaceRenderer<LinkInlineRenderer>(htmlRenderer,
            new CdnMediaLinkRenderer(context, razorPartialRenderer, area, cdnBaseUrl));
        ReplaceRenderer<CodeBlockRenderer>(htmlRenderer, new HighlightCodeBlockRenderer());
        ReplaceRenderer<EmbedRenderer>(htmlRenderer, new EmbedRenderer(services, _markdownPipeline, resolver));

        return htmlRenderer;
    }

    private static void ReplaceRenderer<TExisting>(HtmlRenderer htmlRenderer, IMarkdownObjectRenderer replacement)
        where TExisting : IMarkdownObjectRenderer
    {
        var index = htmlRenderer.ObjectRenderers.FindIndex(r => r is TExisting);
        if (index >= 0)
        {
            htmlRenderer.ObjectRenderers[index] = replacement;
        }
        else
        {
            htmlRenderer.ObjectRenderers.Add(replacement);
        }
    }
}
