using BoothDotDev.Data.Models;
using BoothDotDev.Services;

namespace BoothDotDev.Pages.Admin.Streaks;

/// <summary>
///     Represents the page model for the admin streaks trash page.
/// </summary>
public sealed class Trash : TrashPageModel<Streak>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="streakService">The <see cref="StreakService" />.</param>
    public Trash(StreakService streakService)
        : base(streakService.GetTrashedStreaks, id => streakService.RestoreStreak(id), id => streakService.PermanentlyDeleteStreak(id))
    {
    }

    /// <summary>
    ///     Gets the list of trashed streaks, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed streaks.</value>
    public IReadOnlyList<Streak> Streaks => Items;
}
