using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Extensions.Markdig.Markdown.Callout;

/// <summary>
///     Extension for adding Obsidian-style callouts to a Markdown pipeline.
/// </summary>
internal sealed class CalloutExtension : IMarkdownExtension
{
    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        var parser = pipeline.InlineParsers.Find<CalloutInlineParser>();
        if (parser is null)
        {
            pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new CalloutInlineParser());
        }
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        var blockRenderer = renderer.ObjectRenderers.FindExact<CalloutRenderer>();
        if (blockRenderer is null)
        {
            renderer.ObjectRenderers.InsertBefore<QuoteBlockRenderer>(new CalloutRenderer(pipeline));
        }
    }
}
