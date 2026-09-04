using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Markdown.Subtext;

/// <summary>
///     Extension for adding Discord-style subtext to a Markdown pipeline.
/// </summary>
internal sealed class SubtextExtension : IMarkdownExtension
{
    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.BlockParsers.Contains<SubtextBlockParser>())
        {
            // BlockParserList has no InsertBefore<T>; putting it first is safe since TryOpen
            // bails out immediately for anything that isn't a "-#" prefix
            pipeline.BlockParsers.Insert(0, new SubtextBlockParser());
        }
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<SubtextRenderer>())
        {
            htmlRenderer.ObjectRenderers.InsertBefore<ParagraphRenderer>(new SubtextRenderer());
        }
    }
}
