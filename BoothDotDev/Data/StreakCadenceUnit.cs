using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents the unit of time a check-in streak's cadence is measured in.
/// </summary>
/// <remarks>Only meaningful for a streak whose <see cref="StreakMode" /> is <see cref="StreakMode.CheckIn" />.</remarks>
public enum StreakCadenceUnit
{
    /// <summary>
    ///     The streak's cadence is measured in days.
    /// </summary>
    [PgName("day")]
    Day,

    /// <summary>
    ///     The streak's cadence is measured in weeks.
    /// </summary>
    [PgName("week")]
    Week,

    /// <summary>
    ///     The streak's cadence is measured in months.
    /// </summary>
    [PgName("month")]
    Month,

    /// <summary>
    ///     The streak's cadence is measured in years.
    /// </summary>
    [PgName("year")]
    Year
}
