using BoothDotDev.Data.Models;
using Microsoft.AspNetCore.Components;

namespace BoothDotDev.Pages.Shared.Partials;

/// <summary>
///     Represents a Razor partial that displays an activity entry in the commit motif.
/// </summary>
public sealed class ActivityEntryCommit
{
    /// <summary>
    ///     Gets or sets the activity entry to display.
    /// </summary>
    /// <value>The activity entry to display.</value>
    [Parameter]
    public required ActivityEntry Entry { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this is the first activity entry in the list.
    /// </summary>
    /// <value>A value indicating whether this is the first activity entry in the list.</value>
    [Parameter]
    public bool IsFirst { get; set; }
}
