using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Streaks;

/// <summary>
///     Represents the page model for the admin streaks trash page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Trash : PageModel
{
    private readonly StreakService _streakService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="streakService">The <see cref="StreakService" />.</param>
    public Trash(StreakService streakService)
    {
        _streakService = streakService;
    }

    /// <summary>
    ///     Gets the list of trashed streaks, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed streaks.</value>
    public IReadOnlyList<Streak> Streaks { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Streaks = _streakService.GetTrashedStreaks();
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed streak.
    /// </summary>
    /// <param name="id">The ID of the streak to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        _streakService.RestoreStreak(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting a single trashed streak.
    /// </summary>
    /// <param name="id">The ID of the streak to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDelete(Guid id)
    {
        _streakService.PermanentlyDeleteStreak(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting every selected trashed streak.
    /// </summary>
    /// <param name="ids">The IDs of the streaks to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDeleteBulk(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            _streakService.PermanentlyDeleteStreak(id);
        }

        return RedirectToPage();
    }
}
