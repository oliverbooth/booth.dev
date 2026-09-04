using Cysharp.Text;
using Markdig.Helpers;
using Markdig.Parsers;

namespace BoothDotDev.Markdown.Template;

/// <summary>
///     Represents a Markdown inline parser that handles MediaWiki-style templates.
/// </summary>
public sealed class TemplateInlineParser : InlineParser
{
    private static readonly IReadOnlyDictionary<string, string> EmptyParams =
        new Dictionary<string, string>().AsReadOnly();

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateInlineParser" /> class.
    /// </summary>
    public TemplateInlineParser()
    {
        OpeningCharacters = ['{'];
    }

    /// <inheritdoc />
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        var span = slice.Text.AsSpan()[slice.Start..];
        if (!span.StartsWith("{{"))
        {
            return false;
        }

        var template = ReadUntilClosure(span);
        if (template.IsEmpty)
        {
            return false;
        }

        template = template[2..^2]; // trim {{ and }}
        var name = ReadTemplateName(template, out var argumentSpan);
        var variantIndex = name.IndexOf(':');
        var hasVariant = variantIndex > -1;
        var variant = ReadOnlySpan<char>.Empty;

        if (hasVariant)
        {
            variant = name[(variantIndex + 1)..];
            name = name[..variantIndex];
        }

        if (argumentSpan.IsEmpty)
        {
            processor.Inline = new TemplateInline
            {
                Name = name.ToString(),
                Variant = hasVariant ? variant.ToString() : string.Empty,
                ArgumentString = string.Empty,
                ArgumentList = ArraySegment<string>.Empty,
                Params = EmptyParams
            };

            slice.End = slice.Start;
            slice.Start += template.Length + 4;
            return true;
        }

        var argumentList = new List<string>();
        var paramsList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        ParseArguments(argumentSpan, argumentList, paramsList);

        processor.Inline = new TemplateInline
        {
            Name = name.ToString(),
            Variant = hasVariant ? variant.ToString() : string.Empty,
            ArgumentString = argumentSpan.ToString(),
            ArgumentList = argumentList.AsReadOnly(),
            Params = paramsList.AsReadOnly()
        };

        slice.Start += template.Length + 4;
        return true;
    }

    private static void ParseArguments(ReadOnlySpan<char> argumentSpan,
        IList<string> argumentList,
        IDictionary<string, string> paramsList)
    {
        using var buffer = ZString.CreateUtf8StringBuilder();
        var isKey = true;

        for (var index = 0; index < argumentSpan.Length; index++)
        {
            if (isKey)
            {
                var result = ReadNext(argumentSpan, ref index, false, out var hasValue);
                if (!hasValue)
                {
                    argumentList.Add(result.ToString());
                    continue;
                }

                buffer.Append(result);
                isKey = false;
            }
            else
            {
                var result = ReadNext(argumentSpan, ref index, true, out var _);
                var key = buffer.ToString();
                var value = result.ToString();

                buffer.Clear();
                isKey = true;

                paramsList.Add(key, value);
                argumentList.Add($"{key}={value}");
            }
        }
    }

    private static ReadOnlySpan<char> ReadNext(ReadOnlySpan<char> argumentSpan,
        ref int index,
        bool consumeToken,
        out bool hasValue)
    {
        var isEscaped = false;

        var startIndex = index;
        for (; index < argumentSpan.Length; index++)
        {
            var currentChar = argumentSpan[index];
            switch (currentChar)
            {
                case '\\' when isEscaped:
                    isEscaped = false;
                    break;

                case '\\':
                    isEscaped = true;
                    break;

                case '|' when !isEscaped:
                    hasValue = false;
                    return argumentSpan[startIndex..index];

                case '=' when !isEscaped && !consumeToken:
                    hasValue = true;
                    return argumentSpan[startIndex..index];
            }
        }

        hasValue = false;
        return argumentSpan[startIndex..index];
    }

    private static ReadOnlySpan<char> ReadUntilClosure(ReadOnlySpan<char> input)
    {
        var endIndex = FindClosingBraceIndex(input);
        return endIndex != -1 ? input[..(endIndex + 1)] : ReadOnlySpan<char>.Empty;
    }

    private static ReadOnlySpan<char> ReadTemplateName(ReadOnlySpan<char> input, out ReadOnlySpan<char> argumentSpan)
    {
        var argumentStartIndex = input.IndexOf('|');
        if (argumentStartIndex == -1)
        {
            argumentSpan = Span<char>.Empty;
            return input;
        }

        argumentSpan = input[(argumentStartIndex + 1)..];
        return input[..argumentStartIndex];
    }

    private static int FindClosingBraceIndex(ReadOnlySpan<char> input)
    {
        var openingBraces = 0;
        var closingBraces = 0;

        for (var index = 0; index < input.Length - 1; index++)
        {
            var currentChar = input[index];
            var nextChar = index < input.Length - 2 ? input[index + 1] : '\0';

            if (IsOpeningBraceSequence(currentChar, nextChar))
            {
                openingBraces++;
                index++;
            }
            else if (IsClosingBraceSequence(currentChar, nextChar))
            {
                closingBraces++;
                index++;
            }

            if (openingBraces == closingBraces && openingBraces > 0)
            {
                return index;
            }
        }

        return -1;
    }

    private static bool IsOpeningBraceSequence(char currentChar, char nextChar)
    {
        return currentChar == '{' && nextChar == '{';
    }

    private static bool IsClosingBraceSequence(char currentChar, char nextChar)
    {
        return currentChar == '}' && nextChar == '}';
    }
}
