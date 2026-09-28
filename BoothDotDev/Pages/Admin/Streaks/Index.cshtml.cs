using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Streaks;

/// <summary>
///     Represents the page model for the admin streaks page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly StreakService _streakService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="streakService">The <see cref="StreakService" />.</param>
    public Index(StreakService streakService)
    {
        _streakService = streakService;
    }

    /// <summary>
    ///     Gets the list of streaks, in their curated display order.
    /// </summary>
    /// <value>The list of streaks.</value>
    public IReadOnlyList<Streak> Streaks { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Streaks = _streakService.GetAllStreaks();
    }

    /// <summary>
    ///     Handles the POST request for moving a streak to the trash.
    /// </summary>
    /// <param name="id">The ID of the streak to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        _streakService.TrashStreak(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for saving a new display order for every streak.
    /// </summary>
    /// <param name="ids">Every streak's ID, in its new display order.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostReorder(List<Guid> ids)
    {
        _streakService.Reorder(ids);
        return RedirectToPage();
    }
}
