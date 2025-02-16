using Markdig.Syntax.Inlines;

namespace BoothDotDev.Extensions.Markdig.Markdown.Timestamp;

/// <summary>
///     Represents a Markdown inline element that contains a timestamp.
/// </summary>
public sealed class TimestampInline : Inline
{
    /// <summary>
    ///     Gets or sets the format.
    /// </summary>
    /// <value>The format.</value>
    public TimestampFormat Format { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp.
    /// </summary>
    /// <value>The timestamp.</value>
    public DateTimeOffset Timestamp { get; set; }
}
