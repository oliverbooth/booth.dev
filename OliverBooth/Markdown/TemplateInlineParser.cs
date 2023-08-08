using Cysharp.Text;
using Markdig.Helpers;
using Markdig.Parsers;

namespace OliverBooth.Markdown;

/// <summary>
///     Represents a Markdown inline parser that handles MediaWiki-style templates.
/// </summary>
public sealed class TemplateInlineParser : InlineParser
{
    /// <inheritdoc />
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        ReadOnlySpan<char> span = slice.Text.AsSpan();
        ReadOnlySpan<char> template = span[slice.Start..];

        if (!template.StartsWith("{{"))
        {
            return false;
        }

        int endIndex = template.IndexOf("}}");
        if (endIndex == -1)
        {
            return false;
        }

        template = template[2..endIndex];
        ReadOnlySpan<char> templateName = template;
        int pipeIndex = template.IndexOf('|');
        var templateArgs = new Dictionary<string, string>();
        var rawArgumentString = string.Empty;
        var argumentList = new List<string>();

        if (pipeIndex != -1)
        {
            templateName = templateName[..pipeIndex];
            rawArgumentString = template[(pipeIndex + 1)..].ToString();

            ReadOnlySpan<char> args = template[(pipeIndex + 1)..];

            using Utf8ValueStringBuilder keyBuilder = ZString.CreateUtf8StringBuilder();
            using Utf8ValueStringBuilder valueBuilder = ZString.CreateUtf8StringBuilder();

            var isKey = true;
            var isEscape = false;
            var nestLevel = 0;

            for (var index = 0; index < args.Length; index++)
            {
                char current = args[index];
                var key = keyBuilder.ToString();
                var value = valueBuilder.ToString();

                if (current == '=' && isKey && !isEscape)
                {
                    isKey = false;
                    continue;
                }

                if (current == '{' && index < args.Length - 1 && args[index + 1] == '{')
                {
                    nestLevel++;
                }

                if (current == '}' && index < args.Length - 1 && args[index + 1] == '}')
                {
                    if (nestLevel == 0)
                    {
                        template = template[..(pipeIndex + 1 + index)];
                        args = args[..index];
                    }
                    else
                    {
                        nestLevel--;
                    }
                }

                if (isKey)
                {
                    if (current == '\'')
                    {
                        if (isEscape)
                        {
                            keyBuilder.Append('\'');
                            isEscape = false;
                            continue;
                        }

                        isEscape = !isEscape;

                        if (index == args.Length - 1)
                        {
                            argumentList.Add(isKey ? key : $"{key}={value}");
                            templateArgs.Add(key, value);
                        }

                        continue;
                    }

                    if (current == '|' && !isEscape)
                    {
                        argumentList.Add(key);
                        templateArgs.Add(key, string.Empty);
                        keyBuilder.Clear();

                        if (index == args.Length - 1)
                        {
                            argumentList.Add(isKey ? key : $"{key}={value}");
                            templateArgs.Add(key, value);
                        }

                        continue;
                    }

                    keyBuilder.Append(current);

                    if (index == args.Length - 1)
                    {
                        argumentList.Add(isKey ? key : $"{key}={value}");
                        templateArgs.Add(key, value);
                    }

                    continue;
                }

                if (current == '\'')
                {
                    if (isEscape)
                    {
                        valueBuilder.Append('\'');
                        isEscape = false;
                        continue;
                    }

                    isEscape = !isEscape;
                    continue;
                }

                if (current == '|' && !isEscape)
                {
                    argumentList.Add($"{key}={value}");
                    templateArgs.Add(key, value);
                    keyBuilder.Clear();
                    valueBuilder.Clear();
                    isKey = true;
                    continue;
                }

                valueBuilder.Append(current);
            }
        }

        processor.Inline = new TemplateInline
        {
            Name = templateName.ToString(),
            Params = templateArgs,
            ArgumentString = rawArgumentString,
            ArgumentList = argumentList.ToArray()
        };

        slice.End = slice.Start;
        slice.Start += template.Length + 4;
        return true;
    }
}
