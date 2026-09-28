using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents how a streak's current and best counts are derived from its logged history.
/// </summary>
public enum StreakMode
{
    /// <summary>
    ///     The streak requires an explicit check-in each cadence period to stay alive - missing a period breaks it.
    ///     Used for things that take active effort, e.g. a gym session or a Duolingo lesson.
    /// </summary>
    [PgName("check_in")]
    CheckIn,

    /// <summary>
    ///     The streak counts up automatically from its start date (or its most recent reset) until a reset is
    ///     explicitly logged. Used for abstaining from something, e.g. not smoking, where there's nothing to log on
    ///     an ordinary day.
    /// </summary>
    [PgName("check_out")]
    CheckOut
}
