using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using MarkdownCodeBlock = Markdig.Syntax.CodeBlock;

namespace BoothDotDev.Markdown.CodeBlock;

/// <summary>
///     Extends the stock <see cref="CodeBlockRenderer" /> to surface a fenced code block's <c>h=...</c> highlight trivia as a
///     <c>data-highlight</c> attribute, for client-side application against Prism's rendered tokens.
/// </summary>
public sealed class HighlightCodeBlockRenderer : CodeBlockRenderer
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, MarkdownCodeBlock obj)
    {
        if (obj is FencedCodeBlock { Arguments.Length: > 0 } fenced)
        {
            var arguments = fenced.Arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var highlightSpec = ExtractHighlightBlock(arguments);

            if (highlightSpec is not null)
            {
                obj.GetAttributes().AddPropertyIfNotExist("data-highlight", highlightSpec);
            }

            if (arguments.Contains("nums"))
            {
                obj.GetAttributes().AddPropertyIfNotExist("data-line-numbers", string.Empty);
            }
        }

        base.Write(renderer, obj);
    }

    private static string? ExtractHighlightBlock(string[] arguments)
    {
        foreach (var part in arguments)
        {
            if (part.StartsWith("h=", StringComparison.Ordinal))
            {
                return part;
            }
        }

        return null;
    }
}
