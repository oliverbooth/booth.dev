using Markdig;
using OliverBooth.Markdown.Callout;

namespace OliverBooth.Markdown;

/// <summary>
///     Extension methods for <see cref="MarkdownPipelineBuilder" />.
/// </summary>
internal static class MarkdownExtensions
{
    /// <summary>
    ///     Uses this extension to enable Obsidian-style callouts.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <returns>The modified pipeline.</returns>
    public static MarkdownPipelineBuilder UseCallouts(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.AddIfNotAlready<CalloutExtension>();
        return pipeline;
    }
}
