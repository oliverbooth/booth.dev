namespace BoothDotDev.CodeBlockTrivia.Highlighting;

/// <summary>
///     Parses highlight trivia strings (the <c>h=...</c> portion of a fenced code block's info string) into a structured
///     <see cref="TriviaBlock" />.
/// </summary>
public static class TriviaParser
{
    /// <summary>
    ///     Attempts to parse a single trivia block.
    /// </summary>
    /// <param name="input">The trivia block text, e.g. <c>h=L1-L3,L5@2..^2</c>.</param>
    /// <param name="block">When this method returns, contains the parsed block, if parsing succeeded.</param>
    /// <param name="error">When this method returns, contains the reason parsing failed, if it did.</param>
    /// <returns><see langword="true" /> if parsing succeeded; otherwise, <see langword="false" />.</returns>
    public static bool TryParse(string input, out TriviaBlock block, out TriviaParseError error)
    {
        block = default;

        if (string.IsNullOrEmpty(input))
        {
            error = TriviaParseError.EmptySpec;
            return false;
        }

        if (input.AsSpan().IndexOfAny(' ', '\t') >= 0)
        {
            error = TriviaParseError.UnexpectedWhitespace;
            return false;
        }

        var equalsIndex = input.IndexOf('=');
        if (equalsIndex <= 0)
        {
            error = TriviaParseError.Malformed;
            return false;
        }

        if (equalsIndex == input.Length - 1)
        {
            error = TriviaParseError.EmptySpec;
            return false;
        }

        var kind = input[..equalsIndex];
        var remainder = input.AsSpan(equalsIndex + 1);

        var tokens = new List<HighlightToken>();
        var start = 0;

        while (start < remainder.Length)
        {
            var end = FindTokenEnd(remainder, start);
            var tokenSpan = remainder[start..end];

            if (!TryParseToken(tokenSpan, out var token, out error))
            {
                return false;
            }

            tokens.Add(token);
            start = end + 1; // skip the comma
        }

        if (tokens.Count == 0)
        {
            error = TriviaParseError.EmptySpec;
            return false;
        }

        block = new TriviaBlock(kind, tokens);
        error = TriviaParseError.None;
        return true;
    }

    /// <summary>
    ///     Finds the index of the next top-level comma (i.e. one not nested inside parentheses), or the length
    ///     of the span if none exists.
    /// </summary>
    private static int FindTokenEnd(ReadOnlySpan<char> span, int start)
    {
        var depth = 0;

        for (var i = start; i < span.Length; i++)
        {
            switch (span[i])
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                case ',' when depth == 0:
                    return i;
            }
        }

        return span.Length;
    }

    private static bool TryParseToken(ReadOnlySpan<char> span, out HighlightToken token, out TriviaParseError error)
    {
        token = default;

        var atIndex = span.IndexOf('@');
        var lineSpan = atIndex >= 0 ? span[..atIndex] : span;
        var columnSpan = atIndex >= 0 ? span[(atIndex + 1)..] : ReadOnlySpan<char>.Empty;

        if (!TryParseLineGroup(lineSpan, out var lines, out var isGrouped, out error))
        {
            return false;
        }

        if (atIndex < 0)
        {
            token = new HighlightToken(lines, isGrouped, null);
            error = TriviaParseError.None;
            return true;
        }

        if (!TryParseColumnSpec(columnSpan, out var columns, out error))
        {
            return false;
        }

        token = new HighlightToken(lines, isGrouped, columns);
        error = TriviaParseError.None;
        return true;
    }

    private static bool TryParseLineGroup(ReadOnlySpan<char> span,
        out IReadOnlyList<LineSpec> lines,
        out bool isGrouped,
        out TriviaParseError error)
    {
        lines = Array.Empty<LineSpec>();
        isGrouped = span.Length >= 2 && span[0] == '(' && span[^1] == ')';

        if (isGrouped)
        {
            span = span[1..^1];
        }

        var parsed = new List<LineSpec>();
        var start = 0;

        while (start < span.Length)
        {
            var commaIndex = span[start..].IndexOf(',');
            var end = commaIndex < 0 ? span.Length : start + commaIndex;

            if (!TryParseLineSpec(span[start..end], out var lineSpec, out error))
            {
                return false;
            }

            parsed.Add(lineSpec);
            start = end + 1;
        }

        if (parsed.Count == 0)
        {
            error = TriviaParseError.Malformed;
            return false;
        }

        // a comma-separated list of line-specs is only valid when explicitly grouped in parens;
        // "L1,L2" with no parens is not a line-group, it's two separate tokens at the block level
        if (!isGrouped && parsed.Count > 1)
        {
            error = TriviaParseError.Malformed;
            return false;
        }

        lines = parsed;
        error = TriviaParseError.None;
        return true;
    }

    private static bool TryParseLineSpec(ReadOnlySpan<char> span, out LineSpec lineSpec, out TriviaParseError error)
    {
        lineSpec = default;

        if (span.IsEmpty || span[0] != 'L')
        {
            error = TriviaParseError.Malformed;
            return false;
        }

        span = span[1..]; // drop the 'L'

        var dashIndex = span.IndexOf('-');
        if (dashIndex < 0)
        {
            if (!TryParseBound(span, out var single, out error))
            {
                return false;
            }

            lineSpec = new LineSpec(single, null);
            error = TriviaParseError.None;
            return true;
        }

        var startSpan = span[..dashIndex];
        var endSpan = span[(dashIndex + 1)..];

        // the end side is written "L5", so drop its leading 'L' too
        if (endSpan.IsEmpty || endSpan[0] != 'L')
        {
            error = TriviaParseError.Malformed;
            return false;
        }

        endSpan = endSpan[1..];

        if (!TryParseBound(startSpan, out var start, out error) ||
            !TryParseBound(endSpan, out var end, out error))
        {
            return false;
        }

        if (!start.IsFromEnd && !end.IsFromEnd && end.Value < start.Value)
        {
            error = TriviaParseError.InvertedRange;
            return false;
        }

        lineSpec = new LineSpec(start, end);
        error = TriviaParseError.None;
        return true;
    }

    private static bool TryParseColumnSpec(ReadOnlySpan<char> span,
        out IReadOnlyList<SpecRange> columns,
        out TriviaParseError error)
    {
        columns = Array.Empty<SpecRange>();

        var isGrouped = span.Length >= 2 && span[0] == '(' && span[^1] == ')';
        if (isGrouped)
        {
            span = span[1..^1];
        }

        var ranges = new List<SpecRange>();
        var start = 0;

        while (start < span.Length)
        {
            var commaIndex = span[start..].IndexOf(',');
            var end = commaIndex < 0 ? span.Length : start + commaIndex;

            if (!TryParseRange(span[start..end], out var range, out error))
            {
                return false;
            }

            ranges.Add(range);
            start = end + 1;
        }

        if (ranges.Count == 0)
        {
            error = TriviaParseError.Malformed;
            return false;
        }

        columns = ranges;
        error = TriviaParseError.None;
        return true;
    }

    private static bool TryParseRange(ReadOnlySpan<char> span, out SpecRange range, out TriviaParseError error)
    {
        range = default;

        var dotDotIndex = span.IndexOf("..", StringComparison.Ordinal);
        if (dotDotIndex < 0)
        {
            error = TriviaParseError.Malformed;
            return false;
        }

        var startSpan = span[..dotDotIndex];
        var endSpan = span[(dotDotIndex + 2)..];

        if (!TryParseBound(startSpan, out var start, out error) ||
            !TryParseBound(endSpan, out var end, out error))
        {
            return false;
        }

        if (!start.IsFromEnd && !end.IsFromEnd && end.Value < start.Value)
        {
            error = TriviaParseError.InvertedRange;
            return false;
        }

        range = new SpecRange(start, end);
        error = TriviaParseError.None;
        return true;
    }

    private static bool TryParseBound(ReadOnlySpan<char> span, out SpecBound bound, out TriviaParseError error)
    {
        bound = default;

        var isFromEnd = !span.IsEmpty && span[0] == '^';
        if (isFromEnd)
        {
            span = span[1..];
        }

        if (!int.TryParse(span, out var value) || value <= 0)
        {
            error = TriviaParseError.InvalidNumber;
            return false;
        }

        bound = new SpecBound(value, isFromEnd);
        error = TriviaParseError.None;
        return true;
    }
}
