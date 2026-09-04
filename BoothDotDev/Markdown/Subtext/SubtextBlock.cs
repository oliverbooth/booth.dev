using Markdig.Parsers;
using Markdig.Syntax;

namespace BoothDotDev.Markdown.Subtext;

/// <summary>
///     Represents a Discord-style subtext block, i.e. a line prefixed with <c>-#</c>.
/// </summary>
internal sealed class SubtextBlock : LeafBlock
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SubtextBlock" /> class.
    /// </summary>
    /// <param name="parser">The parser which created this block.</param>
    public SubtextBlock(BlockParser? parser) : base(parser)
    {
        ProcessInlines = true;
    }
}
