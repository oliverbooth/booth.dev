using BoothDotDev.Markdown.Link;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Markdown.Embed;

/// <summary>
///     Represents a Markdown object renderer for handling Obsidian-style file embeds (<c>![[filename]]</c>).
///     HTML and Markdown files are transcluded directly from disk; any other extension (images, audio, video) falls back to CDN
///     media resolution.
/// </summary>
public sealed class EmbedRenderer : HtmlObjectRenderer<EmbedInline>
{
    private readonly ILogger<EmbedRenderer> _logger;
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly CdnMediaResolver _resolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbedRenderer" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="markdownPipeline">The markdown pipeline, used to render transcluded <c>.md</c> embeds.</param>
    /// <param name="resolver">The resolver used to render non-document (image/video/audio) embeds via the CDN.</param>
    public EmbedRenderer(IServiceProvider serviceProvider, MarkdownPipeline markdownPipeline, CdnMediaResolver resolver)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<EmbedRenderer>>();
        _markdownPipeline = markdownPipeline;
        _resolver = resolver;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, EmbedInline embed)
    {
        switch (Path.GetExtension(embed.FileName).ToLowerInvariant())
        {
            case ".html":
                WriteLocalDocument(renderer, embed.FileName, true);
                break;

            case ".md":
                WriteLocalDocument(renderer, embed.FileName, false);
                break;

            default:
                // _resolver is null for the placeholder EmbedRenderer the pipeline installs by default (see
                // EmbedExtension) - real renders replace it with one bound to the current post's render context
                // via MarkdownRenderingService.CreateHtmlRenderer, but plain-text-only passes (Markdown.ToPlainText,
                // used for OG/RSS descriptions) never go through that replacement. A media embed has no meaningful
                // plain-text representation anyway, so skip it rather than crash.
                if (_resolver is null)
                {
                    _logger.LogDebug("Skipping media embed {FileName} - no resolver bound (likely a plain-text render pass)",
                        embed.FileName);
                    break;
                }

                var result = _resolver.RenderMediaAsync(embed.FileName, null, null).GetAwaiter().GetResult();
                renderer.Write(result);
                break;
        }
    }

    /// <summary>
    ///     Reads a local embed file from disk and writes its content to the renderer, either as raw HTML or rendered from
    ///     Markdown depending on <paramref name="raw" />.
    /// </summary>
    /// <param name="renderer">The active <see cref="HtmlRenderer" /> to write to.</param>
    /// <param name="fileName">The bare filename of the embed, relative to <c>data/embeds</c>.</param>
    /// <param name="raw">
    ///     <see langword="true" /> to write the file's content verbatim (HTML embeds); <see langword="false" /> to render it as
    ///     Markdown first (Markdown embeds).
    /// </param>
    private void WriteLocalDocument(HtmlRenderer renderer, string fileName, bool raw)
    {
        var filename = $"data/embeds/{fileName}";

        if (!File.Exists(filename))
        {
            _logger.LogWarning("Embed file {Filename} does not exist", filename);
            return;
        }

        if (raw)
        {
            _logger.LogDebug("Embedding HTML file {Filename}", filename);
            renderer.Write(File.ReadAllText(filename));
        }
        else
        {
            _logger.LogDebug("Embedding Markdown file {Filename}", filename);
            var markdown = File.ReadAllText(filename);
            renderer.Write(Markdig.Markdown.ToHtml(markdown, _markdownPipeline));
        }
    }
}
