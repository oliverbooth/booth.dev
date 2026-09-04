using Markdig.Helpers;
using Markdig.Parsers;

namespace BoothDotDev.Markdown.Spoiler;

/// <summary>
///     Represents a Markdown inline parser that matches Discord-style spoiler markers (<c>||</c>).
/// </summary>
/// <remarks>
///     Must run ahead of Markdig's pipe-table inline parser, which otherwise claims every <c>|</c> unconditionally
///     (including in ordinary prose, for cell splitting) - see <see cref="SpoilerExtension" />.
///     A single, unpaired <c>|</c> is left alone (returns <see langword="false" />) so table syntax keeps working.
/// </remarks>
internal sealed class SpoilerInlineParser : InlineParser
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SpoilerInlineParser" /> class.
    /// </summary>
    public SpoilerInlineParser()
    {
        OpeningCharacters = ['|'];
    }

    /// <inheritdoc />
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        if (slice.PeekChar(1) != '|')
        {
            return false;
        }

        slice.SkipChar(); // first '|'
        slice.SkipChar(); // second '|'

        // closed rather than left open: whether this marker turns out to be an "open" or a "close" isn't decided
        // here, only once its pair is found, so it shouldn't swallow subsequent content as its own children either way
        processor.Inline = new SpoilerDelimiterInline(this) { IsClosed = true };
        return true;
    }
}
