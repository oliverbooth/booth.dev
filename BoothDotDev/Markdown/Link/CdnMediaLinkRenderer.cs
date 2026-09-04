using System.Net;
using BoothDotDev.Services;
using BoothDotDev.Views;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.StaticFiles;

namespace BoothDotDev.Markdown.Link;

/// <summary>
///     Represents a Markdown inline renderer that handles CDN media links.
/// </summary>
public sealed class CdnMediaLinkRenderer : LinkInlineRenderer
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
    private readonly string _area;
    private readonly string _baseUrl;
    private readonly RazorPartialRenderer _razorPartialRenderer;
    private readonly MarkdownRenderContext _renderContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CdnMediaLinkRenderer" /> class.
    /// </summary>
    /// <param name="renderContext">The rendering context.</param>
    /// <param name="razorPartialRenderer">The Razor partial renderer.</param>
    /// <param name="area">The area.</param>
    /// <param name="baseUrl">The base URL of the CDN.</param>
    public CdnMediaLinkRenderer(MarkdownRenderContext renderContext,
        RazorPartialRenderer razorPartialRenderer,
        string area,
        string baseUrl)
    {
        _renderContext = renderContext;
        _razorPartialRenderer = razorPartialRenderer;
        _area = area;
        _baseUrl = baseUrl;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, LinkInline link)
    {
        if (!link.IsImage)
        {
            base.Write(renderer, link);
            return;
        }

        var mediaKind = ResolveMediaKind(link.Url);
        var cdnUrl = ResolveCdnUrl(link.Url, mediaKind);

        var partialName = mediaKind switch
        {
            MediaKind.Video => "_Video",
            MediaKind.Audio => "_Audio",
            _ => "_Image"
        };

        var model = new MediaLinkModel
        {
            Url = cdnUrl, Alt = ExtractAltText(renderer, link), Title = link.Title, MimeType = GetMimeType(link.Url)
        };
        var result = _razorPartialRenderer.RenderToStringAsync(partialName, model).GetAwaiter().GetResult();
        renderer.Write(result);
    }

    private static string? GetMimeType(string? url)
    {
        if (url is null)
        {
            return null;
        }

        ContentTypeProvider.TryGetContentType(url, out var contentType);
        return contentType;
    }

    private static MediaKind ResolveMediaKind(string? url)
    {
        var extension = Path.GetExtension(url)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
        return extension switch
        {
            "png" or "jpg" or "jpeg" or "gif" or "webp" or "svg" => MediaKind.Image,
            "mp4" or "webm" or "mov" => MediaKind.Video,
            "mp3" or "wav" or "ogg" or "flac" => MediaKind.Audio,
            _ => MediaKind.Misc
        };
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

        // typographic replacements (SmartyPants curly quotes, dashes, ellipses) are written as literal HTML
        // entity text, correct for embedding in HTML body markup. decode it back.
        return WebUtility.HtmlDecode(altWriter.ToString());
    }

    private string ResolveCdnUrl(string? url, MediaKind mediaKind)
    {
        var uuid = _renderContext.Id;
        var date = _renderContext.Date;

        return $"{_baseUrl}/{_area}/{mediaKind.ToString().ToLowerInvariant()}/{date:yyyy/MM}/{uuid:N}/{url}";
    }

    private enum MediaKind
    {
        Image,
        Video,
        Audio,
        Misc
    }
}
