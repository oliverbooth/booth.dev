using BoothDotDev.Services;
using BoothDotDev.Views;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace BoothDotDev.Markdown.Link;

/// <summary>
///     Represents a Markdown inline renderer that handles CDN image links.
/// </summary>
public sealed class CdnImageLinkRenderer : LinkInlineRenderer
{
    private const string BaseUrl = "https://cdn.booth.dev";
    private readonly MarkdownRenderContext _renderContext;
    private readonly RazorPartialRenderer _razorPartialRenderer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CdnImageLinkRenderer" /> class.
    /// </summary>
    /// <param name="renderContext">The rendering context.</param>
    /// <param name="razorPartialRenderer">The Razor partial renderer.</param>
    public CdnImageLinkRenderer(MarkdownRenderContext renderContext,
        RazorPartialRenderer razorPartialRenderer)
    {
        _renderContext = renderContext;
        _razorPartialRenderer = razorPartialRenderer;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, LinkInline link)
    {
        if (!link.IsImage)
        {
            base.Write(renderer, link);
            return;
        }

        var model = new ImageTemplate { Url = ResolveCdnUrl(link.Url), Alt = ExtractAltText(renderer, link), Title = link.Title };
        var result = _razorPartialRenderer.RenderToStringAsync("_ImageTemplate", model).GetAwaiter().GetResult();
        renderer.Write(result);
    }

    private static string ExtractAltText(HtmlRenderer renderer, LinkInline link)
    {
        using var altWriter = new StringWriter();
        var altRenderer = new HtmlRenderer(altWriter) { EnableHtmlForInline = false };

        foreach (var objectRenderer in renderer.ObjectRenderers)
        {
            altRenderer.ObjectRenderers.Add(objectRenderer);
        }

        altRenderer.WriteChildren(link);
        altWriter.Flush();

        return altWriter.ToString();
    }

    private string ResolveCdnUrl(string? url)
    {
        // TODO: swap out blog/img for <category>/<type> resolution
        return $"{BaseUrl}/blog/img/{_renderContext.Date:yyyy/MM}/{_renderContext.Id:N}/{url}";
    }
}
