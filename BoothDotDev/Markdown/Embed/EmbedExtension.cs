using Markdig;
using Markdig.Renderers;

namespace BoothDotDev.Markdown.Embed;

/// <summary>
///     Extension for adding Obsidian-style file embeds to a Markdown pipeline.
/// </summary>
internal sealed class EmbedExtension : IMarkdownExtension
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbedExtension" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public EmbedExtension(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.InlineParsers.Contains<EmbedInlineParser>())
        {
            pipeline.InlineParsers.Insert(0, new EmbedInlineParser());
        }
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<EmbedRenderer>())
        {
            htmlRenderer.ObjectRenderers.Insert(0, new EmbedRenderer(_serviceProvider, pipeline, resolver: null!));
        }
    }
}
