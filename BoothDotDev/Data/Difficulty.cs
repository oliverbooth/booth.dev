using System.ComponentModel;
using NpgsqlTypes;

namespace BoothDotDev.Data;

/// <summary>
///     Represents how hard a development challenge is.
/// </summary>
public enum Difficulty
{
    /// <summary>
    ///     The challenge is approachable for a newcomer.
    /// </summary>
    [PgName("easy")] [Description("The challenge is approachable for a newcomer.")]
    Easy,

    /// <summary>
    ///     The challenge takes some experience.
    /// </summary>
    [PgName("intermediate")] [Description("The challenge takes some experience.")]
    Intermediate,

    /// <summary>
    ///     The challenge is demanding, even for an experienced developer.
    /// </summary>
    [PgName("hard")] [Description("The challenge is demanding, even for an experienced developer.")]
    Hard,

    /// <summary>
    ///     The challenge is brutal.
    /// </summary>
    [PgName("insane")] [Description("The challenge is brutal.")]
    Insane
}
