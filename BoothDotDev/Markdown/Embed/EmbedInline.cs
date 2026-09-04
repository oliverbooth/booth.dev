using Markdig.Syntax.Inlines;

namespace BoothDotDev.Markdown.Embed;

/// <summary>
///     Represents a Markdown inline element that handles Obsidian-style file embeds (<c>![[filename]]</c>).
/// </summary>
public sealed class EmbedInline : Inline
{
    /// <summary>
    ///     Gets or initializes the filename of the embedded file.
    /// </summary>
    /// <value>The filename of the embedded file.</value>
    public required string FileName { get; init; }
}
