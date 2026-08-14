using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;
using BoothDotDev.Markdown;
using BoothDotDev.Markdown.CodeBlock;
using BoothDotDev.Markdown.Link;
using Humanizer;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using MD = Markdig.Markdown;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for rendering Markdown content into HTML.
/// </summary>
public sealed class MarkdownRenderingService
{
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
    public string Render(string body, Guid id, DateTimeOffset published, string area)
    {
        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        using var writer = new StringWriter();
        using var scope = _serviceScopeFactory.CreateScope();

        HtmlRenderer htmlRenderer = CreateHtmlRenderer(writer, area, id, published, scope.ServiceProvider);

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
            return MD.ToPlainText(markdown.Excerpt, _markdownPipeline);
        }

        var body = markdown.Body;
        var moreIndex = body.IndexOf("<!--more-->", StringComparison.Ordinal);

        if (moreIndex == -1)
        {
            var excerpt = body.Truncate(255, "...");
            wasTrimmed = body.Length > 255;
            return MD.ToPlainText(excerpt, _markdownPipeline);
        }

        wasTrimmed = true;
        return MD.ToPlainText(body[..moreIndex], _markdownPipeline);
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

        ReplaceRenderer<LinkInlineRenderer>(htmlRenderer, new CdnMediaLinkRenderer(context, razorPartialRenderer, area));
        ReplaceRenderer<CodeBlockRenderer>(htmlRenderer, new HighlightCodeBlockRenderer());

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
