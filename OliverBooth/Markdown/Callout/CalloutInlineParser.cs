using System.Reflection;
using Cysharp.Text;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace OliverBooth.Markdown.Callout;

/// <summary>
///     An inline parser for Obsidian-style callouts (<c>[!NOTE]</c> etc.)
/// </summary>
internal sealed class CalloutInlineParser : InlineParser
{
    // ugly hack to access internal method
    private static readonly MethodInfo ReplaceParentContainerMethod =
        typeof(InlineProcessor).GetMethod("ReplaceParentContainer", BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CalloutInlineParser" /> class.
    /// </summary>
    public CalloutInlineParser()
    {
        OpeningCharacters = ['['];
    }

    /// <inheritdoc />
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        // We expect the alert to be the first child of a quote block. Example:
        // > [!NOTE]
        // > This is a note
        if (processor.Block is not ParagraphBlock { Parent: QuoteBlock quoteBlock } paragraphBlock ||
            paragraphBlock.Inline?.FirstChild != null)
        {
            return false;
        }

        StringSlice cache = slice;
        char current = slice.NextChar();

        if (current != '!')
        {
            slice = cache;
            return false;
        }

        current = slice.NextChar(); // skip !

        int start = slice.Start;
        int end = start;

        while (current.IsAlphaUpper())
        {
            end = slice.Start;
            current = slice.NextChar();
        }

        if (current != ']' || start == end)
        {
            slice = cache;
            return false;
        }

        var type = new StringSlice(slice.Text, start, end);
        current = slice.NextChar(); // skip ]
        start = slice.Start;

        bool fold = false;
        if (current == '-')
        {
            fold = true;
            current = slice.NextChar(); // skip -
            start = slice.Start;
        }

        ReadTitle(current, ref slice, out StringSlice title, out end);

        var callout = new CalloutBlock(type)
        {
            Foldable = fold,
            Span = quoteBlock.Span,
            TrailingWhitespaceTrivia = new StringSlice(slice.Text, start, end),
            Line = quoteBlock.Line,
            Column = quoteBlock.Column,
            Title = title
        };

        AddAttributes(callout, type);
        ReplaceQuoteBlock(processor, quoteBlock, callout);
        return true;
    }

    private static void ReadTitle(char startChar, ref StringSlice slice, out StringSlice title, out int end)
    {
        using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();

        char current = startChar;
        while (true)
        {
            if (current is not ('\0' or '\r' or '\n'))
            {
                builder.Append(current);
                current = slice.NextChar();
                continue;
            }

            end = slice.Start;
            if (HandleCharacter(ref slice, ref end, ref current))
            {
                continue;
            }

            break;
        }

        title = new StringSlice(builder.ToString(), 0, builder.Length);
    }

    private static bool HandleCharacter(ref StringSlice slice, ref int end, ref char current)
    {
        switch (current)
        {
            case '\r':
                current = slice.NextChar(); // skip \r

                if (current is not ('\0' or '\n'))
                {
                    return true;
                }

                end = slice.Start;
                if (current == '\n')
                {
                    slice.NextChar(); // skip \n
                }

                break;

            case '\n':
                slice.NextChar(); // skip \n
                break;
        }

        return false;
    }

    private static void AddAttributes(IMarkdownObject callout, StringSlice type)
    {
        HtmlAttributes attributes = callout.GetAttributes();
        attributes.AddClass("callout");
        attributes.AddProperty("data-callout", type.AsSpan().ToString().ToLowerInvariant());
    }

    private static void ReplaceQuoteBlock(InlineProcessor processor, QuoteBlock quoteBlock, CalloutBlock callout)
    {
        ContainerBlock? parentQuoteBlock = quoteBlock.Parent;
        if (parentQuoteBlock is null)
        {
            return;
        }

        int indexOfQuoteBlock = parentQuoteBlock.IndexOf(quoteBlock);
        parentQuoteBlock[indexOfQuoteBlock] = callout;

        while (quoteBlock.Count > 0)
        {
            var block = quoteBlock[0];
            quoteBlock.RemoveAt(0);
            callout.Add(block);
        }

        ReplaceParentContainerMethod.Invoke(processor, [quoteBlock, callout]);
        // ReplaceParentContainer(processor, quoteBlock, callout);
    }
}
