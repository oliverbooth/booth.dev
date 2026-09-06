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
    private readonly TraktAuthService _traktAuthService;
    private readonly TraktSyncService _traktSyncService;
    private readonly WatchlistService _watchlistService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="watchlistService">The watchlist service.</param>
    /// <param name="traktAuthService">The Trakt auth service.</param>
    /// <param name="traktSyncService">The Trakt sync service.</param>
    public Index(WatchlistService watchlistService, TraktAuthService traktAuthService, TraktSyncService traktSyncService)
    {
        _watchlistService = watchlistService;
        _traktAuthService = traktAuthService;
        _traktSyncService = traktSyncService;
    }

    /// <summary>
    ///     Gets a value indicating whether Trakt has been connected.
    /// </summary>
    /// <value><see langword="true" /> if Trakt has been connected; otherwise, <see langword="false" />.</value>
    public bool IsTraktConnected { get; private set; }

    /// <summary>
    ///     Gets the pending Trakt device authorization challenge, if a "Connect Trakt" flow is in progress.
    /// </summary>
    /// <value>The pending challenge, or <see langword="null" /> if there isn't one.</value>
    public TraktDeviceChallenge? PendingChallenge { get; private set; }

    /// <summary>
    ///     Gets or sets an error from the most recent Trakt action, if any.
    /// </summary>
    /// <value>The error message, or <see langword="null" /> if the most recent action succeeded (or none was made).</value>
    [TempData]
    public string? TraktError { get; set; }

    /// <summary>
    ///     Gets or sets a status message from the most recent Trakt action, if any.
    /// </summary>
    /// <value>The status message, or <see langword="null" /> if none is pending.</value>
    [TempData]
    public string? TraktMessage { get; set; }

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
        IsTraktConnected = _traktAuthService.IsConnected();
        PendingChallenge = _traktAuthService.GetPendingChallenge();
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

    /// <summary>
    ///     Handles the POST request for starting the Trakt device authorization flow.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public async Task<IActionResult> OnPostConnectTraktAsync(CancellationToken cancellationToken)
    {
        var result = await _traktAuthService.StartDeviceAuthAsync(cancellationToken);
        if (result.IsFailed)
        {
            TraktError = result.Errors[0].Message;
        }

        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for checking whether a pending Trakt device authorization has completed.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public async Task<IActionResult> OnPostCheckTraktAuthAsync(CancellationToken cancellationToken)
    {
        var result = await _traktAuthService.CheckDeviceAuthAsync(cancellationToken);
        if (result.IsFailed)
        {
            TraktError = result.Errors[0].Message;
        }
        else
        {
            TraktMessage = "Trakt connected.";
        }

        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for pulling the watchlist and watched status from Trakt.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public async Task<IActionResult> OnPostSyncTraktAsync(CancellationToken cancellationToken)
    {
        var result = await _traktSyncService.SyncAsync(cancellationToken);
        if (result.IsFailed)
        {
            TraktError = result.Errors[0].Message;
        }
        else
        {
            var summary = result.Value;
            TraktMessage = $"Pulled from Trakt: {summary.Added} added, {summary.Promoted} promoted, {summary.Adopted} linked to existing entries.";
        }

        return RedirectToPage();
    }
}
