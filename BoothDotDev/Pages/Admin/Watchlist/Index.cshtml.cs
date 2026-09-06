using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Watchlist;

/// <summary>
///     Represents the page model for the admin watchlist page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly WatchlistService _watchlistService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="watchlistService">The watchlist service.</param>
    public Index(WatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    /// <summary>
    ///     Gets every item on the watchlist.
    /// </summary>
    /// <value>Every item on the watchlist.</value>
    public IReadOnlyCollection<Watchable> Watchables { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Watchables = _watchlistService.GetAllWatchables();
    }

    /// <summary>
    ///     Handles the POST request for changing an item's state.
    /// </summary>
    /// <param name="id">The ID of the item to update.</param>
    /// <param name="state">The new state.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSetState(Guid id, WatchableState state)
    {
        _watchlistService.SetState(id, state);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for removing an item from the watchlist.
    /// </summary>
    /// <param name="id">The ID of the item to remove.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        _watchlistService.DeleteWatchable(id);
        return RedirectToPage();
    }
}
