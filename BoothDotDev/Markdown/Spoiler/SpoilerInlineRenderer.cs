using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Markdown.Spoiler;

/// <summary>
///     Represents an HTML renderer which renders a <see cref="SpoilerInline" />.
/// </summary>
internal sealed class SpoilerInlineRenderer : HtmlObjectRenderer<SpoilerInline>
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, SpoilerInline obj)
    {
        if (renderer.EnableHtmlForInline)
        {
            renderer.Write("<span").WriteAttributes(obj).Write('>');
        }

        renderer.WriteChildren(obj);

        if (renderer.EnableHtmlForInline)
        {
            renderer.Write("</span>");
        }
    }
}
