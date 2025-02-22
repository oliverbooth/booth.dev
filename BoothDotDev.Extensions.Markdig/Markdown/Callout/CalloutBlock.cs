using Markdig.Helpers;
using Markdig.Syntax;

namespace BoothDotDev.Extensions.Markdig.Markdown.Callout;

/// <summary>
///     Represents a callout block.
/// </summary>
internal sealed class CalloutBlock : QuoteBlock
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CalloutBlock" /> class.
    /// </summary>
    /// <param name="type">The type of the callout.</param>
    public CalloutBlock(StringSlice type) : base(null)
    {
        Type = type;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this callout is foldable.
    /// </summary>
    /// <value><see langword="true" /> if this callout is foldable; otherwise, <see langword="false" />.</value>
    public bool Foldable { get; set; }

    /// <summary>
    ///     Gets or sets the title of the callout.
    /// </summary>
    /// <value>The title of the callout.</value>
    public StringSlice Title { get; set; }

    /// <summary>
    ///     Gets or sets the trailing whitespace trivia.
    /// </summary>
    /// <value>The trailing whitespace trivia.</value>
    public StringSlice TrailingWhitespaceTrivia { get; set; }

    /// <summary>
    ///     Gets or sets the type of the callout.
    /// </summary>
    /// <value>The type of the callout.</value>
    public StringSlice Type { get; set; }
}
