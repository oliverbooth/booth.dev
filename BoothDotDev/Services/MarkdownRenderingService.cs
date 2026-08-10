using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;
using Humanizer;
using Markdig;
using MD = Markdig.Markdown;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for rendering Markdown content into HTML.
/// </summary>
public sealed class MarkdownRenderingService
{
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MarkdownRenderingService" /> class.
    /// </summary>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" /> to use for rendering Markdown.</param>
    public MarkdownRenderingService(MarkdownPipeline markdownPipeline)
    {
        _markdownPipeline = markdownPipeline;
    }

    /// <summary>
    ///     Renders the body of a <see cref="IMarkdownBody" /> as HTML using the configured Markdown pipeline.
    /// </summary>
    /// <param name="body">The <see cref="IMarkdownBody" /> to render.</param>
    /// <returns>The HTML content of the rendered content.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="body" /> is <see langword="null" />.</exception>
    public string Render(IMarkdownBody body)
    {
        if (body is null)
        {
            throw new ArgumentNullException(nameof(body));
        }

        return MD.ToHtml(body.Body, _markdownPipeline);
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
    public string RenderTableOfContents(IMarkdownBody markdown, HttpRequest? request)
    {
        if (markdown is null)
        {
            throw new ArgumentNullException(nameof(markdown));
        }

        List<TocItem> items = MarkdownTocBuilder.BuildToc(markdown.Body);
        return MarkdownTocBuilder.RenderTocAsHtml(items, request);
    }
}
