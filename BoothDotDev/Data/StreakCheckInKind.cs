using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents whether a logged day of a check-in streak counts toward its displayed number.
/// </summary>
public enum StreakCheckInKind
{
    /// <summary>
    ///     The thing was actually done on this day.
    /// </summary>
    [PgName("completed")]
    Completed,

    /// <summary>
    ///     The thing wasn't done on this day, but the streak is excused rather than broken (e.g. a Duolingo streak freeze).
    /// </summary>
    [PgName("frozen")]
    Frozen
}
