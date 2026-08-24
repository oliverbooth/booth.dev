using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.ValueConverters;

/// <summary>
///     A value converter that normalizes Markdown text by trimming whitespace and ensuring a single trailing newline, as well as
///     converting NBSP characters to regular spaces.
/// </summary>
/// <remarks>
///     Markdig can refuse to close a block (e.g. a fenced code block) that ends at the very end of the document with no following
///     blank line. Trimming and appending exactly one trailing newline on write guarantees every Markdown field is padded,
///     without ever accumulating more than the one blank line it needs.
/// </remarks>
internal sealed class MarkdownValueConverter()
    : ValueConverter<string, string>(v => $"{v.Replace('\u00A0', ' ').Trim()}\n", v => v);
