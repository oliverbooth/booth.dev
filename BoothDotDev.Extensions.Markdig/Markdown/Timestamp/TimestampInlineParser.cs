using Markdig.Helpers;
using Markdig.Parsers;

namespace BoothDotDev.Extensions.Markdig.Markdown.Timestamp;

/// <summary>
///     Represents a Markdown inline parser that matches Discord-style timestamps.
/// </summary>
public sealed class TimestampInlineParser : InlineParser
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TimestampInlineParser" /> class.
    /// </summary>
    public TimestampInlineParser()
    {
        OpeningCharacters = new[] { '<' };
    }

    /// <inheritdoc />
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        // Previous char must be a space
        if (!slice.PeekCharExtra(-1).IsWhiteSpaceOrZero())
        {
            return false;
        }

        ReadOnlySpan<char> span = slice.Text.AsSpan(slice.Start, slice.Length);

        if (!TryConsumeTimestamp(span, out ReadOnlySpan<char> rawTimestamp, out char format))
        {
            return false;
        }

        if (!long.TryParse(rawTimestamp, out long timestamp))
        {
            return false;
        }

        bool hasFormat = format != '\0';
        processor.Inline = new TimestampInline
        {
            Format = (TimestampFormat)format,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp)
        };

        int paddingCount = hasFormat ? 6 : 4; // <t:*> or optionally <t:*:*>
        slice.Start += rawTimestamp.Length + paddingCount;
        return true;
    }

    private bool TryConsumeTimestamp(ReadOnlySpan<char> source,
        out ReadOnlySpan<char> timestamp,
        out char format)
    {
        timestamp = default;
        format = default;

        if (!source.StartsWith("<t:")) return false;
        timestamp = source[3..];

        if (timestamp.IndexOf('>') == -1)
        {
            timestamp = default;
            return false;
        }

        int delimiterIndex = timestamp.IndexOf(':');
        if (delimiterIndex == 0)
        {
            // invalid format <t::*>
            timestamp = default;
            return false;
        }

        if (delimiterIndex == -1)
        {
            // no format, default to relative
            format = 'R';
            timestamp = timestamp[..^1]; // trim >
        }
        else
        {
            // use specified format
            format = timestamp[^2];
            timestamp = timestamp[..^3];
        }

        return true;
    }
}
