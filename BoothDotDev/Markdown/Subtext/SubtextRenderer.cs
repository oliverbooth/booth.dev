using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Markdown.Subtext;

/// <summary>
///     Represents an HTML renderer which renders a <see cref="SubtextBlock" />.
/// </summary>
internal sealed class SubtextRenderer : HtmlObjectRenderer<SubtextBlock>
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, SubtextBlock block)
    {
        if (!renderer.EnableHtmlForBlock)
        {
            renderer.WriteLeafInline(block);
            return;
        }

        if (!renderer.IsFirstInContainer)
        {
            renderer.EnsureLine();
        }

        renderer.Write("<p class=\"md-subtext\">");
        renderer.WriteLeafInline(block);
        renderer.WriteLine("</p>");
    }
}
