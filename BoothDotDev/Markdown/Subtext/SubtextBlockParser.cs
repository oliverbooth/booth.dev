using Markdig.Parsers;
using Markdig.Syntax;

namespace BoothDotDev.Markdown.Subtext;

/// <summary>
///     Represents a Markdown block parser that matches Discord-style subtext, i.e. a line prefixed with <c>-#</c>.
/// </summary>
internal sealed class SubtextBlockParser : BlockParser
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SubtextBlockParser" /> class.
    /// </summary>
    public SubtextBlockParser()
    {
        OpeningCharacters = ['-'];
    }

    /// <inheritdoc />
    public override BlockState TryOpen(BlockProcessor processor)
    {
        if (processor.IsCodeIndent || processor.CurrentChar != '-' || processor.PeekChar(1) != '#')
        {
            return BlockState.None;
        }

        // a space is required after the -#, unless there's no content at all
        var afterMarker = processor.PeekChar(2);
        if (afterMarker != ' ' && afterMarker != '\t' && afterMarker != '\0')
        {
            return BlockState.None;
        }

        var column = processor.Column;
        var start = processor.Start;

        processor.NextChar(); // skip '-'
        processor.NextChar(); // skip '#'
        if (processor.CurrentChar is ' ' or '\t')
        {
            processor.NextChar(); // skip the single space separating the marker from the content
        }

        var block = new SubtextBlock(this) { Column = column, Span = new SourceSpan(start, processor.Line.End) };

        processor.NewBlocks.Push(block);

        // subtext is always a single line, so don't try to continue this block on subsequent lines
        return BlockState.Break;
    }
}
