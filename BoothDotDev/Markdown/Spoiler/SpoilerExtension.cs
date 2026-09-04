using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BoothDotDev.Markdown.Spoiler;

/// <summary>
///     Extension for adding Discord-style spoilers (<c>||text||</c>) to a Markdown pipeline.
/// </summary>
internal sealed class SpoilerExtension : IMarkdownExtension
{
    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.InlineParsers.Contains<SpoilerInlineParser>())
        {
            // must run ahead of PipeTableParser, which otherwise claims every '|' unconditionally; InsertBefore<T>
            // isn't used since UsePipeTables() may not have registered it yet depending on extension order, and
            // Insert(0, ...) wins that race regardless of that order
            pipeline.InlineParsers.Insert(0, new SpoilerInlineParser());
            pipeline.DocumentProcessed += ResolveSpoilers;
        }
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<SpoilerInlineRenderer>())
        {
            htmlRenderer.ObjectRenderers.Insert(0, new SpoilerInlineRenderer());
        }
    }

    /// <summary>
    ///     Pairs up <see cref="SpoilerDelimiterInline" /> markers within every leaf block's inline tree and wraps each
    ///     pair's content in a <see cref="SpoilerInline" />.
    /// </summary>
    private static void ResolveSpoilers(MarkdownDocument document)
    {
        foreach (var block in document.Descendants<LeafBlock>())
        {
            if (block.Inline is { } container)
            {
                ResolveSpoilers(container);
            }
        }
    }

    /// <summary>
    ///     Collects the <see cref="SpoilerDelimiterInline" /> markers directly under <paramref name="container" />, recursing
    ///     into any nested container (e.g. the content of an already-resolved emphasis or link) first.
    /// </summary>
    private static void ResolveSpoilers(ContainerInline container)
    {
        List<SpoilerDelimiterInline>? markers = null;

        for (var inline = container.FirstChild; inline is not null; inline = inline.NextSibling)
        {
            switch (inline)
            {
                case SpoilerDelimiterInline marker:
                    (markers ??= []).Add(marker);
                    break;

                case ContainerInline nested:
                    ResolveSpoilers(nested);
                    break;
            }
        }

        if (markers is not null)
        {
            Pair(markers);
        }
    }

    /// <summary>
    ///     Pairs consecutive markers (1st + 2nd, 3rd + 4th, ...), wrapping each pair's content in a <see cref="SpoilerInline" />;
    ///     a trailing unpaired marker falls back to a literal <c>||</c>.
    /// </summary>
    private static void Pair(List<SpoilerDelimiterInline> markers)
    {
        var i = 0;
        for (; i + 1 < markers.Count; i += 2)
        {
            var open = markers[i];
            var close = markers[i + 1];

            if (open.Parent is null || open.Parent != close.Parent)
            {
                // mismatched nesting (e.g. an emphasis span opened between them and never closed) - not resolvable
                // without the pairing partner as a plain sibling, so leave both as literal text
                open.ReplaceByLiteral();
                close.ReplaceByLiteral();
                continue;
            }

            var spoiler = new SpoilerInline();
            spoiler.GetAttributes().AddClass("spoiler");

            open.ReplaceBy(spoiler);
            for (var current = spoiler.NextSibling; current is not null && current != close;)
            {
                var next = current.NextSibling;
                current.Remove();
                spoiler.AppendChild(current);
                current = next;
            }

            close.Remove();
        }

        if (i < markers.Count)
        {
            markers[i].ReplaceByLiteral();
        }
    }
}
