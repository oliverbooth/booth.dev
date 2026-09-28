namespace BoothDotDev.Data;

/// <summary>
///     Represents a streak's computed current and best counts, derived from its logged history.
/// </summary>
/// <param name="Current">
///     The length of the streak's current run, or zero if it's currently broken (a check-in streak that's gone overdue for its
///     cadence).
/// </param>
/// <param name="Best">The length of the longest run the streak has ever had, including an ongoing one.</param>
public sealed record StreakStats(int Current, int Best);
