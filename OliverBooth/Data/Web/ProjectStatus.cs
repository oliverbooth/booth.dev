using System.ComponentModel;

namespace OliverBooth.Data.Web;

/// <summary>
///     Represents the status of a project.
/// </summary>
public enum ProjectStatus
{
    [Description("The project is currently being worked on.")]
    Ongoing,

    [Description("The project is on an indefinite hiatus.")]
    Hiatus,

    [Description("The project is no longer being worked on.")]
    Past,

    [Description("The project has been retired with no plans for completion.")]
    Retired,
}
