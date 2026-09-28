namespace BoothDotDev.Data;

/// <summary>
///     Represents which layout a streak's detail page uses to show its history, chosen from its cadence.
/// </summary>
public enum StreakViewKind
{
    /// <summary>
    ///     A month grid of dots, one per day. Used for a daily check-in streak and every check-out streak.
    /// </summary>
    DailyDots,

    /// <summary>
    ///     A grid of dots, one per calendar period (week, month, quarter, or year), paged by year or decade.
    /// </summary>
    PeriodDots,

    /// <summary>
    ///     A paged list of the most recent occurrences. Used for a cadence too sparse or irregular for a dot grid to
    ///     read naturally, e.g. every 3 days or every 5 years.
    /// </summary>
    Timeline
}

/// <summary>
///     Represents a single day or period's state in a streak's detail page.
/// </summary>
public enum StreakDayState
{
    /// <summary>
    ///     Outside the streak's meaningful range - before its first logged day, or in the future.
    /// </summary>
    Blank,

    /// <summary>
    ///     The thing was done.
    /// </summary>
    Completed,

    /// <summary>
    ///     The day was excused (frozen) rather than broken.
    /// </summary>
    Frozen,

    /// <summary>
    ///     A check-in streak's day or period that passed with nothing logged.
    /// </summary>
    Missed,

    /// <summary>
    ///     A check-out streak was broken on this day.
    /// </summary>
    Reset
}

/// <summary>
///     Represents one dot or list item in a streak's detail page.
/// </summary>
/// <param name="Label">The text shown on or under the dot (a day number, a week/month/quarter/year label, or a date).</param>
/// <param name="State">The day or period's state.</param>
/// <param name="IsToday">A value indicating whether this is today's cell.</param>
public sealed record StreakDetailCell(string Label, StreakDayState State, bool IsToday);
