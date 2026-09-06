using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which manages the movie/TV watchlist.
/// </summary>
public sealed class WatchlistService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WatchlistService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public WatchlistService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets the watchlist items with the specified state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>A collection of watchlist items in the specified state.</returns>
    public IReadOnlyCollection<Watchable> GetWatchables(WatchableState state)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Watchables.Where(w => w.State == state).ToArray();
    }

    /// <summary>
    ///     Gets every item on the watchlist.
    /// </summary>
    /// <returns>A collection of every watchlist item.</returns>
    public IReadOnlyCollection<Watchable> GetAllWatchables()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Watchables.OrderBy(w => w.Title).ToArray();
    }

    /// <summary>
    ///     Gets the total number of items on the watchlist.
    /// </summary>
    /// <returns>The total number of items on the watchlist.</returns>
    public int GetWatchableCount()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Watchables.Count();
    }

    /// <summary>
    ///     Gets a single watchlist item by its ID.
    /// </summary>
    /// <param name="id">The ID of the item.</param>
    /// <returns>A <see cref="Result{T}" /> containing the item, or an error if no item with the specified ID was found.</returns>
    public Result<Watchable> GetWatchableById(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var watchable = context.Watchables.Find(id);
        return watchable is null ? Result.Fail($"No watchlist item with ID '{id}' was found.") : Result.Ok(watchable);
    }

    /// <summary>
    ///     Adds a new item to the watchlist.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="state">The state.</param>
    /// <returns>A <see cref="Result{T}" /> containing the added item.</returns>
    public Result<Watchable> AddWatchable(string title, WatchableKind kind, WatchableState state)
    {
        var watchable = new Watchable
        {
            Id = Guid.NewGuid(),
            Title = title,
            Kind = kind,
            State = state,
            Source = WatchableSource.Manual
        };

        using var context = _dbContextFactory.CreateDbContext();
        context.Watchables.Add(watchable);
        context.SaveChanges();
        return Result.Ok(watchable);
    }

    /// <summary>
    ///     Updates an existing watchlist item.
    /// </summary>
    /// <param name="id">The ID of the item to update.</param>
    /// <param name="title">The new title.</param>
    /// <param name="kind">The new kind.</param>
    /// <param name="state">The new state.</param>
    /// <returns>A <see cref="Result{T}" /> containing the updated item, or an error if no item with the specified ID was found.</returns>
    public Result<Watchable> UpdateWatchable(Guid id, string title, WatchableKind kind, WatchableState state)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var watchable = context.Watchables.Find(id);
        if (watchable is null)
        {
            return Result.Fail($"No watchlist item with ID '{id}' was found.");
        }

        watchable.Title = title;
        watchable.Kind = kind;
        watchable.State = state;
        context.SaveChanges();
        return Result.Ok(watchable);
    }

    /// <summary>
    ///     Sets the state of a watchlist item.
    /// </summary>
    /// <param name="id">The ID of the item to update.</param>
    /// <param name="state">The new state.</param>
    /// <returns>A <see cref="Result" /> indicating success, or an error if no item with the specified ID was found.</returns>
    public Result SetState(Guid id, WatchableState state)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var watchable = context.Watchables.Find(id);
        if (watchable is null)
        {
            return Result.Fail($"No watchlist item with ID '{id}' was found.");
        }

        watchable.State = state;
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Reconciles the watchlist against a fresh pull from Trakt. Adds any watchlist item that isn't already
    ///     tracked as <see cref="WatchableState.PlanToWatch" />, and adds or promotes any watched item to
    ///     <see cref="WatchableState.Watched" /> - except an item already <see cref="WatchableState.Watching" />.
    /// </summary>
    /// <param name="watchlist">The items on the Trakt watchlist.</param>
    /// <param name="watched">The items Trakt considers fully watched (movies watched at all; shows watched in full).</param>
    /// <returns>A summary of how many entries were added or promoted.</returns>
    public WatchlistSyncSummary ReconcileFromTrakt(IReadOnlyList<TraktMediaRef> watchlist, IReadOnlyList<TraktMediaRef> watched)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var existing = context.Watchables
            .Where(w => w.TraktId != null)
            .ToDictionary(w => (TraktId: w.TraktId!.Value, w.Kind));

        var added = 0;
        var promoted = 0;

        foreach (var item in watchlist)
        {
            if (existing.ContainsKey((item.TraktId, item.Kind)))
            {
                continue;
            }

            var watchable = new Watchable
            {
                Id = Guid.NewGuid(),
                Title = item.Title,
                Kind = item.Kind,
                State = WatchableState.PlanToWatch,
                Source = WatchableSource.Trakt,
                TraktId = item.TraktId
            };
            context.Watchables.Add(watchable);
            existing[(item.TraktId, item.Kind)] = watchable;
            added++;
        }

        foreach (var item in watched)
        {
            if (existing.TryGetValue((item.TraktId, item.Kind), out var watchable))
            {
                if (watchable.State == WatchableState.PlanToWatch)
                {
                    watchable.State = WatchableState.Watched;
                    promoted++;
                }

                // Watching is left alone regardless - it only ever changes by hand. Watched is already correct
                continue;
            }

            context.Watchables.Add(new Watchable
            {
                Id = Guid.NewGuid(),
                Title = item.Title,
                Kind = item.Kind,
                State = WatchableState.Watched,
                Source = WatchableSource.Trakt,
                TraktId = item.TraktId
            });
            added++;
        }

        context.SaveChanges();
        return new WatchlistSyncSummary(added, promoted);
    }

    /// <summary>
    ///     Removes an item from the watchlist.
    /// </summary>
    /// <param name="id">The ID of the item to remove.</param>
    /// <returns>A <see cref="Result" /> indicating success, or an error if no item with the specified ID was found.</returns>
    public Result DeleteWatchable(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var watchable = context.Watchables.Find(id);
        if (watchable is null)
        {
            return Result.Fail($"No watchlist item with ID '{id}' was found.");
        }

        context.Watchables.Remove(watchable);
        context.SaveChanges();
        return Result.Ok();
    }
}

/// <summary>
///     Represents a reference to a single movie or show from Trakt, as needed to reconcile it against the watchlist.
/// </summary>
/// <param name="TraktId">The Trakt ID of the item.</param>
/// <param name="Kind">The kind of the item.</param>
/// <param name="Title">The title of the item.</param>
public sealed record TraktMediaRef(int TraktId, WatchableKind Kind, string Title);

/// <summary>
///     Represents the result of reconciling the watchlist against a fresh pull from Trakt.
/// </summary>
/// <param name="Added">The number of entries newly added.</param>
/// <param name="Promoted">
///     The number of entries promoted from <see cref="WatchableState.PlanToWatch" /> to
///     <see cref="WatchableState.Watched" />.
/// </param>
public sealed record WatchlistSyncSummary(int Added, int Promoted);
