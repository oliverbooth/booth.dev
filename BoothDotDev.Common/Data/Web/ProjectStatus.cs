using System.ComponentModel;
using NpgsqlTypes;

namespace BoothDotDev.Common.Data.Web;

/// <summary>
///     Represents the status of a project.
/// </summary>
public enum ProjectStatus
{
    /// <summary>
    ///     The project is currently being worked on.
    /// </summary>
    [PgName("ongoing"), Description("The project is currently being worked on.")]
    Ongoing,

    /// <summary>
    ///     The project is on an indefinite hiatus.
    /// </summary>
    [PgName("hiatus"), Description("The project is on an indefinite hiatus.")]
    Hiatus,

    /// <summary>
    ///     The project is no longer being worked on.
    /// </summary>
    [PgName("past"), Description("The project is no longer being worked on.")]
    Past,

    /// <summary>
    ///     The project has been retired with no plans for completion.
    /// </summary>
    [PgName("retired"), Description("The project has been retired with no plans for completion.")]
    Retired,
}
