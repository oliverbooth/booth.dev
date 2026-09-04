using Markdig.Parsers;
using Markdig.Syntax.Inlines;

namespace BoothDotDev.Markdown.Spoiler;

/// <summary>
///     Represents an unresolved <c>||</c> marker, paired up with the next one in the same container by
///     <see cref="SpoilerExtension" /> once the whole document has been parsed.
/// </summary>
internal sealed class SpoilerDelimiterInline : DelimiterInline
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SpoilerDelimiterInline" /> class.
    /// </summary>
    /// <param name="parser">The parser which created this delimiter.</param>
    public SpoilerDelimiterInline(InlineParser parser) : base(parser)
    {
    }

    /// <inheritdoc />
    public override string ToLiteral()
    {
        return "||";
    }
}
