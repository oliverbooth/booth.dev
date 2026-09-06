using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

/// <summary>
///     Represents the page model for the /now page.
/// </summary>
public sealed class Now : PageModel
{
    private readonly PhoneStatusService _phoneStatusService;
    private readonly ReadingListService _readingListService;
    private readonly WatchlistService _watchlistService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Now" /> class.
    /// </summary>
    /// <param name="readingListService">The reading list service.</param>
    /// <param name="watchlistService">The watchlist service.</param>
    /// <param name="phoneStatusService">The phone status service.</param>
    public Now(ReadingListService readingListService, WatchlistService watchlistService, PhoneStatusService phoneStatusService)
    {
        _readingListService = readingListService;
        _watchlistService = watchlistService;
        _phoneStatusService = phoneStatusService;
    }

    /// <summary>
    ///     Gets the books currently being read.
    /// </summary>
    /// <value>The books currently being read.</value>
    public IReadOnlyCollection<Book> CurrentlyReading { get; private set; } = [];

    /// <summary>
    ///     Gets the movies/shows currently being watched.
    /// </summary>
    /// <value>The movies/shows currently being watched.</value>
    public IReadOnlyCollection<Watchable> CurrentlyWatching { get; private set; } = [];

    /// <summary>
    ///     Gets the most recently reported phone status.
    /// </summary>
    /// <value>The most recent phone status, or <see langword="null" /> if none has been reported, or it's gone stale.</value>
    public PhoneStatusSnapshot? PhoneStatus { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        CurrentlyReading = _readingListService.GetBooks(BookState.Reading);
        CurrentlyWatching = _watchlistService.GetWatchables(WatchableState.Watching);
        PhoneStatus = _phoneStatusService.GetStatus();
    }
}
