using HtmlAgilityPack;
using Humanizer;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BoothDotDev.Markdown.Callout;

/// <summary>
///     Represents an HTML renderer which renders a <see cref="CalloutBlock" />.
/// </summary>
internal sealed class CalloutRenderer : HtmlObjectRenderer<CalloutBlock>
{
    private static readonly Dictionary<string, string> CalloutTypes = new()
    {
        ["NOTE"] = "pencil",
        ["ABSTRACT"] = "clipboard-list",
        ["INFO"] = "info-circle",
        ["TODO"] = "circle-check",
        ["TIP"] = "flame",
        ["IMPORTANT"] = "flame",
        ["SUCCESS"] = "check",
        ["QUESTION"] = "help-circle",
        ["WARNING"] = "alert-triangle",
        ["FAILURE"] = "x",
        ["DANGER"] = "bolt",
        ["BUG"] = "bug",
        ["EXAMPLE"] = "list",
        ["CITE"] = "quote",
        ["UPDATE"] = "calendar-check"
    };

    private readonly MarkdownPipeline _pipeline;

    public CalloutRenderer(MarkdownPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, CalloutBlock block)
    {
        renderer.EnsureLine();
        if (renderer.EnableHtmlForBlock)
        {
            RenderAsHtml(renderer, block, _pipeline);
        }
        else
        {
            RenderAsText(renderer, block);
        }

        renderer.EnsureLine();
    }

    private static void RenderAsHtml(HtmlRenderer renderer, CalloutBlock block, MarkdownPipeline pipeline)
    {
        var title = block.Title.Text;
        var type = block.Type.AsSpan();
        Span<char> upperType = stackalloc char[type.Length];
        type.ToUpperInvariant(upperType);

        if (!CalloutTypes.TryGetValue(upperType.ToString(), out var tablerIcon))
        {
            tablerIcon = "pencil";
        }

        var typeString = type.ToString().ToLowerInvariant();

        renderer.Write(block.Foldable
            ? $"<details class=\"callout\" data-callout=\"{typeString}\""
            : $"<div class=\"callout\" data-callout=\"{typeString}\"");

        renderer.Write('>');
        renderer.Write(block.Foldable
            ? "<summary class=\"callout-title\"><i class=\"ti ti-"
            : "<div class=\"callout-title\"><i class=\"ti ti-");

        renderer.Write(tablerIcon);
        renderer.Write("\"></i> ");

        var calloutTitle = title.Length == 0 ? typeString.Humanize(LetterCasing.Sentence) : title;
        WriteTitle(renderer, pipeline, calloutTitle);

        renderer.WriteLine(block.Foldable ? "</summary>" : "</div>");

        renderer.Write("<div class=\"callout-body\">");
        renderer.WriteChildren(block);
        renderer.WriteLine("</div>");
        renderer.WriteLine(block.Foldable ? "</details>" : "</div>");
        renderer.EnsureLine();
    }

    private static void WriteTitle(TextRendererBase renderer, MarkdownPipeline pipeline, string calloutTitle)
    {
        var html = Markdig.Markdown.ToHtml(calloutTitle, pipeline);
        var document = new HtmlDocument();
        document.LoadHtml(html);
        if (document.DocumentNode.FirstChild is { Name: "p" } child)
        {
            document.DocumentNode.InnerHtml = child.InnerHtml;
        }

        document.Save(renderer.Writer);
    }

    private static void RenderAsText(HtmlRenderer renderer, CalloutBlock block)
    {
        var title = block.Title.Text;
        var type = block.Type.AsSpan();
        renderer.WriteLine(title.Length == 0 ? type.ToString().ToUpperInvariant() : title.ToUpperInvariant());
        renderer.WriteChildren(block);
        renderer.EnsureLine();
    }
}
