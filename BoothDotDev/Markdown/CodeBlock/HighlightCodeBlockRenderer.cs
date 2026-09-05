using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using MarkdownCodeBlock = Markdig.Syntax.CodeBlock;

namespace BoothDotDev.Markdown.CodeBlock;

/// <summary>
///     Extends the stock <see cref="CodeBlockRenderer" /> to surface a fenced code block's <c>h=...</c> highlight trivia as a
///     <c>data-highlight</c> attribute, for client-side application against Prism's rendered tokens; also flags
///     <c>manim-2d</c>/<c>manim-3d</c> blocks for client-side rendering as a manim-web scene, <c>vexflow</c> blocks for
///     client-side rendering as music notation, and <c>mermaid</c> blocks for client-side rendering as a diagram. All
///     three render tabbed alongside their own source by default; <c>no-source</c> renders just the result with no
///     source tab, and <c>no-render</c> skips rendering entirely, leaving a plain highlighted codeblock.
/// </summary>
public sealed class HighlightCodeBlockRenderer : CodeBlockRenderer
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, MarkdownCodeBlock obj)
    {
        string? manimDimension = null;
        var isVexFlow = false;

        if (obj is FencedCodeBlock fenced)
        {
            var arguments = fenced.Arguments?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
            var highlightSpec = ExtractHighlightBlock(arguments);

            if (highlightSpec is not null)
            {
                obj.GetAttributes().AddPropertyIfNotExist("data-highlight", highlightSpec);
            }

            if (arguments.Contains("nums"))
            {
                obj.GetAttributes().AddPropertyIfNotExist("data-line-numbers", string.Empty);
            }

            if (arguments.Contains("wrap"))
            {
                obj.GetAttributes().AddPropertyIfNotExist("data-wrap", string.Empty);
            }

            if (!arguments.Contains("no-render"))
            {
                manimDimension = ExtractManimDimension(arguments);
                if (manimDimension is not null)
                {
                    obj.GetAttributes().AddPropertyIfNotExist("data-manim", manimDimension);
                }

                isVexFlow = arguments.Contains("vexflow");
                if (isVexFlow)
                {
                    obj.GetAttributes().AddPropertyIfNotExist("data-vexflow", string.Empty);
                }

                var isMermaid = fenced.Info == "mermaid";
                if (isMermaid)
                {
                    obj.GetAttributes().AddPropertyIfNotExist("data-mermaid", string.Empty);
                }

                if ((manimDimension is not null || isVexFlow || isMermaid) && arguments.Contains("no-source"))
                {
                    obj.GetAttributes().AddPropertyIfNotExist("data-no-source", string.Empty);
                }
            }
        }

        base.Write(renderer, obj);

        if (manimDimension is not null || isVexFlow)
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
