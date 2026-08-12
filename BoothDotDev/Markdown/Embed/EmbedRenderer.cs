using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Markdown.Embed;

/// <summary>
///     Represents a Markdown object renderer for handling Obsidian-style file embeds.
/// </summary>
public sealed class EmbedRenderer : HtmlObjectRenderer<EmbedInline>
{
    private readonly ILogger<EmbedRenderer> _logger;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbedRenderer" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="markdownPipeline">The markdown pipeline.</param>
    public EmbedRenderer(IServiceProvider serviceProvider, MarkdownPipeline markdownPipeline)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<EmbedRenderer>>();
        _markdownPipeline = markdownPipeline;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, EmbedInline embed)
    {
        var filename = $"data/embeds/{embed.FileName}";

        if (!File.Exists(filename))
        {
            _logger.LogWarning("Embed file {Filename} does not exist", filename);
            return;
        }

        switch (Path.GetExtension(filename))
        {
            case ".html":
                _logger.LogDebug("Embedding HTML file {Filename}", filename);
                renderer.Write(File.ReadAllText(filename));
                break;

            case ".md":
                _logger.LogDebug("Embedding Markdown file {Filename}", filename);
                var markdown = File.ReadAllText(filename);
                renderer.Write(Markdig.Markdown.ToHtml(markdown, _markdownPipeline));
                break;

            default:
                _logger.LogWarning("Embed file {Filename} has an unsupported extension", filename);
                break;
        }
    }
}
