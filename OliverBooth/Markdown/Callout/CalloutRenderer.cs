using Humanizer;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace OliverBooth.Markdown.Callout;

/// <summary>
///     Represents an HTML renderer which renders a <see cref="CalloutBlock" />.
/// </summary>
internal sealed class CalloutRenderer : HtmlObjectRenderer<CalloutBlock>
{
    private static readonly Dictionary<string, string> CalloutTypes = new()
    {
        ["NOTE"] = "pencil",
        ["ABSTRACT"] = "clipboard-list",
        ["INFO"] = "info",
        ["TODO"] = "circle-check",
        ["TIP"] = "flame",
        ["SUCCESS"] = "check",
        ["QUESTION"] = "circle-help",
        ["WARNING"] = "triangle-alert",
        ["FAILURE"] = "x",
        ["DANGER"] = "zap",
        ["BUG"] = "bug",
        ["EXAMPLE"] = "list",
        ["CITE"] = "quote",
        ["UPDATE"] = "calendar-check",
    };

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, CalloutBlock block)
    {
        renderer.EnsureLine();
        if (renderer.EnableHtmlForBlock)
        {
            RenderAsHtml(renderer, block);
        }
        else
        {
            RenderAsText(renderer, block);
        }

        renderer.EnsureLine();
    }

    private static void RenderAsHtml(HtmlRenderer renderer, CalloutBlock block)
    {
        string title = block.Title.Text;
        ReadOnlySpan<char> type = block.Type.AsSpan();
        Span<char> upperType = stackalloc char[type.Length];
        type.ToUpperInvariant(upperType);

        if (!CalloutTypes.TryGetValue(upperType.ToString(), out string? lucideClass))
        {
            lucideClass = "pencil";
        }

        var typeString = type.ToString().ToLowerInvariant();

        renderer.Write($"<div class=\"callout\" data-callout=\"{typeString}\">");
        renderer.Write("<div class=\"callout-title\"><i data-lucide=\"");
        renderer.Write(lucideClass);
        renderer.Write("\"></i> ");

        renderer.Write(title.Length == 0 ? typeString.Humanize(LetterCasing.Sentence) : title);
        renderer.WriteLine("</div>");
        
        renderer.WriteChildren(block);

        renderer.WriteLine("</div>");
        renderer.EnsureLine();
    }

    private static void RenderAsText(HtmlRenderer renderer, CalloutBlock block)
    {
        string title = block.Title.Text;
        ReadOnlySpan<char> type = block.Type.AsSpan();
        renderer.WriteLine(title.Length == 0 ? type.ToString().ToUpperInvariant() : title.ToUpperInvariant());
        renderer.WriteChildren(block);
        renderer.EnsureLine();
    }
}
