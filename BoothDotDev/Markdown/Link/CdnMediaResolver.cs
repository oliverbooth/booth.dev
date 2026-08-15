using BoothDotDev.Services;
using BoothDotDev.Views;
using Microsoft.AspNetCore.StaticFiles;

namespace BoothDotDev.Markdown.Link;

/// <summary>
///     Resolves a bare media filename (as used in CDN links and Obsidian-style embeds) to a fully-qualified CDN URL, and renders
///     the appropriate HTML partial for the resolved media kind.
/// </summary>
public sealed class CdnMediaResolver
{
    /// <summary>
    ///     Represents the kind of media a resolved CDN URL points to, used to select the correct rendering partial.
    /// </summary>
    public enum MediaKind
    {
        /// <summary>The media is an image (png, jpg, jpeg, gif, webp, svg).</summary>
        Image,

        /// <summary>The media is a video (mp4, webm, mov).</summary>
        Video,

        /// <summary>The media is an audio file (mp3, wav, ogg, flac).</summary>
        Audio,

        /// <summary>The media is of an unrecognized or unsupported type.</summary>
        Misc
    }

    private const string BaseUrl = "https://cdn.booth.dev";
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
    private readonly MarkdownRenderContext _renderContext;
    private readonly RazorPartialRenderer _razorPartialRenderer;
    private readonly string _area;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CdnMediaResolver" /> class.
    /// </summary>
    /// <param name="renderContext">The rendering context, supplying the containing post's ID and published date.</param>
    /// <param name="razorPartialRenderer">The Razor partial renderer used to render media partials to a string.</param>
    /// <param name="area">The content area (e.g. blog, tutorials, projects) used in the CDN path.</param>
    public CdnMediaResolver(MarkdownRenderContext renderContext, RazorPartialRenderer razorPartialRenderer, string area)
    {
        _renderContext = renderContext;
        _razorPartialRenderer = razorPartialRenderer;
        _area = area;
    }

    /// <summary>
    ///     Determines the <see cref="MediaKind" /> of a URL or filename based on its file extension.
    /// </summary>
    /// <param name="url">The URL or filename to inspect.</param>
    /// <returns>The resolved <see cref="MediaKind" />.</returns>
    public static MediaKind ResolveMediaKind(string? url)
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

    /// <summary>
    ///     Resolves the MIME type of a URL or filename based on its file extension.
    /// </summary>
    /// <param name="url">The URL or filename to inspect.</param>
    /// <returns>
    ///     The resolved MIME type, or <see langword="null" /> if <paramref name="url" /> is <see langword="null" /> or
    ///     unrecognized.
    /// </returns>
    private static string? GetMimeType(string? url)
    {
        if (url is null)
        {
            return null;
        }

        ContentTypeProvider.TryGetContentType(url, out var contentType);
        return contentType;
    }

    /// <summary>
    ///     Builds the fully-qualified CDN URL for a bare filename, using the containing post's ID and published date from the
    ///     <see cref="MarkdownRenderContext" />.
    /// </summary>
    /// <param name="url">The bare filename to resolve.</param>
    /// <param name="mediaKind">The <see cref="MediaKind" /> of the file, used to select the CDN path segment.</param>
    /// <returns>The fully-qualified CDN URL.</returns>
    public string ResolveCdnUrl(string? url, MediaKind mediaKind)
    {
        return
            $"{BaseUrl}/{_area}/{mediaKind.ToString().ToLowerInvariant()}/{_renderContext.Date:yyyy/MM}/{_renderContext.Id:N}/{url}";
    }

    /// <summary>
    ///     Resolves a bare filename to a CDN URL and renders the appropriate media partial (image, video, or audio) to an HTML
    ///     string.
    /// </summary>
    /// <param name="url">The bare filename to resolve and render.</param>
    /// <param name="alt">The alt text for the media, or <see langword="null" /> if not available.</param>
    /// <param name="title">The title for the media, or <see langword="null" /> if not available.</param>
    /// <returns>A task that resolves to the rendered HTML for the media element.</returns>
    public async Task<string> RenderMediaAsync(string url, string? alt, string? title)
    {
        var mediaKind = ResolveMediaKind(url);
        var cdnUrl = ResolveCdnUrl(url, mediaKind);

        var partialName = mediaKind switch
        {
            MediaKind.Video => "_Video",
            MediaKind.Audio => "_Audio",
            _ => "_Image"
        };

        var model = new MediaLinkModel { Url = cdnUrl, Alt = alt, Title = title, MimeType = GetMimeType(url) };
        return await _razorPartialRenderer.RenderToStringAsync(partialName, model);
    }
}
