namespace BoothDotDev.Extensions.Markdig.Markdown.Timestamp;

/// <summary>
///     An enumeration of timestamp formats.
/// </summary>
public enum TimestampFormat
{
    /// <summary>
    ///     Short time format. Example: 12:00
    /// </summary>
    ShortTime = 't',

    /// <summary>
    ///     Long time format. Example: 12:00:00
    /// </summary>
    LongTime = 'T',

    /// <summary>
    ///     Short date format. Example: 1/1/2000
    /// </summary>
    ShortDate = 'd',

    /// <summary>
    ///     Long date format. Example: 1 January 2000
    /// </summary>
    LongDate = 'D',

    /// <summary>
    ///     Short date/time format. Example: 1 January 2000 at 12:00
    /// </summary>
    LongDateShortTime = 'f',

    /// <summary>
    ///     Long date/time format. Example: Saturday, 1 January 2000 at 12:00
    /// </summary>
    LongDateTime = 'F',

    /// <summary>
    ///     Relative date/time format. Example: 1 second ago
    /// </summary>
    Relative = 'R',
}
