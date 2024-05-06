using Markdig;
using Markdig.Renderers;

namespace OliverBooth.Extensions.Markdig.Markdown.Timestamp;

/// <summary>
///     Represents a Markdig extension that supports Discord-style timestamps.
/// </summary>
public class TimestampExtension : IMarkdownExtension
{
    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.InlineParsers.AddIfNotAlready<TimestampInlineParser>();
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            htmlRenderer.ObjectRenderers.AddIfNotAlready<TimestampRenderer>();
        }
    }
}
