using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace BoothDotDev.Markdown.Spoiler;

/// <summary>
///     Represents an HTML renderer which renders a <see cref="SpoilerInline" />.
/// </summary>
internal sealed class SpoilerInlineRenderer : HtmlObjectRenderer<SpoilerInline>
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, SpoilerInline obj)
    {
        var tag = ContainsMedia(obj) ? "div" : "span";

        if (renderer.EnableHtmlForInline)
        {
            renderer.Write($"<{tag}").WriteAttributes(obj).Write('>');
        }

        renderer.WriteChildren(obj);

        if (renderer.EnableHtmlForInline)
        {
            renderer.Write($"</{tag}>");
        }
    }

    private static bool ContainsMedia(ContainerInline container)
    {
        for (var inline = container.FirstChild; inline is not null; inline = inline.NextSibling)
        {
            switch (inline)
            {
                case LinkInline { IsImage: true }:
                    return true;

                case ContainerInline nested when ContainsMedia(nested):
                    return true;
            }
        }

        return false;
    }
}
