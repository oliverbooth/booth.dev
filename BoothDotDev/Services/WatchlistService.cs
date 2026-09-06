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
        var watchable = new Watchable { Id = Guid.NewGuid(), Title = title, Kind = kind, State = state };

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
