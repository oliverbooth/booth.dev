using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using MarkdownCodeBlock = Markdig.Syntax.CodeBlock;

namespace BoothDotDev.Markdown.CodeBlock;

/// <summary>
///     Extends the stock <see cref="CodeBlockRenderer" /> to surface a fenced code block's <c>h=...</c> highlight trivia as a
///     <c>data-highlight</c> attribute, for client-side application against Prism's rendered tokens; also flags
///     <c>manim-2d</c>/<c>manim-3d</c> blocks for client-side rendering as a manim-web scene.
/// </summary>
public sealed class HighlightCodeBlockRenderer : CodeBlockRenderer
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, MarkdownCodeBlock obj)
    {
        var manimDimension = (string?)null;

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

            manimDimension = ExtractManimDimension(arguments);
            if (manimDimension is not null)
            {
                obj.GetAttributes().AddPropertyIfNotExist("data-manim", manimDimension);
            }
        }

        base.Write(renderer, obj);

        if (manimDimension is not null)
        {
            // client-side rendering replaces this codeblock with a live scene; readers without JavaScript never see
            // that happen, so they're left looking at raw code with no indication a visualization was intended
            renderer.Write("<noscript>This code renders as an interactive scene with JavaScript enabled.</noscript>");
        }
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

    private static string? ExtractManimDimension(string[] arguments)
    {
        foreach (var part in arguments)
        {
            switch (part)
            {
                case "manim-2d":
                    return "2d";

                case "manim-3d":
                    return "3d";
            }
        }

        return null;
    }
}
