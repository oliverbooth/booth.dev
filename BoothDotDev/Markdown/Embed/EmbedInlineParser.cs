using Markdig.Helpers;
using Markdig.Parsers;

namespace BoothDotDev.Markdown.Embed;

/// <summary>
///     Represents a Markdown inline parser that handles Obsidian-style file embeds (<c>![[filename]]</c>).
/// </summary>
public sealed class EmbedInlineParser : InlineParser
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbedInlineParser" /> class.
    /// </summary>
    public EmbedInlineParser()
    {
        OpeningCharacters = ['!'];
    }

    /// <inheritdoc />
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        ReadOnlySpan<char> span = slice.Text.AsSpan()[slice.Start..];
        if (!span.StartsWith("![["))
        {
            return false;
        }

        var closeIndex = span.IndexOf("]]", StringComparison.Ordinal);
        if (closeIndex < 0)
        {
            return false;
        }

        ReadOnlySpan<char> inner = span[3..closeIndex]; // trim "![[" and "]]"
        if (inner.IsEmpty)
        {
            return false;
        }

        processor.Inline = new EmbedInline { FileName = inner.ToString() };

        slice.Start += closeIndex + 2;
        return true;
    }
}
