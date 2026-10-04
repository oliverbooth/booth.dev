using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which manages the game list.
/// </summary>
public sealed class GamelistService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GamelistService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public GamelistService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets the games with the specified state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>A collection of games in the specified state.</returns>
    public IReadOnlyCollection<Playable> GetPlayables(PlayableState state)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Playables.Where(p => p.State == state).ToArray();
    }

    /// <summary>
    ///     Gets every game on the game list.
    /// </summary>
    /// <returns>A collection of every game.</returns>
    public IReadOnlyCollection<Playable> GetAllPlayables()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Playables.OrderBy(p => p.Title).ToArray();
    }

    /// <summary>
    ///     Gets the total number of games on the game list.
    /// </summary>
    /// <returns>The total number of games.</returns>
    public int GetPlayableCount()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Playables.Count();
    }

    /// <summary>
    ///     Gets a single game by its ID.
    /// </summary>
    /// <param name="id">The ID of the game.</param>
    /// <returns>A <see cref="Result{T}" /> containing the game, or an error if no game with the specified ID was found.</returns>
    public Result<Playable> GetPlayableById(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var playable = context.Playables.Find(id);
        return playable is null ? Result.Fail($"No game with ID '{id}' was found.") : Result.Ok(playable);
    }

    /// <summary>
    ///     Adds a new game to the game list.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="state">The state.</param>
    /// <param name="igdbSlug">The IGDB slug to link the game to, if any.</param>
    /// <param name="platforms">The platforms the game has been played on.</param>
    /// <returns>A <see cref="Result{T}" /> containing the added game, or an error if the slug is already linked.</returns>
    public Result<Playable> AddPlayable(string title, PlayableState state, string? igdbSlug,
        IEnumerable<GamePlatform> platforms)
    {
        using var context = _dbContextFactory.CreateDbContext();
        if (FindSlugConflict(context, igdbSlug, null) is { } conflict)
        {
            return Result.Fail(conflict);
        }

        var playable = new Playable
        {
            Id = Guid.NewGuid(), Title = title, State = state, IgdbSlug = igdbSlug, Platforms = Normalize(platforms)
        };
        context.Playables.Add(playable);
        context.SaveChanges();
        return Result.Ok(playable);
    }

    /// <summary>
    ///     Updates an existing game.
    /// </summary>
    /// <param name="id">The ID of the game to update.</param>
    /// <param name="title">The new title.</param>
    /// <param name="state">The new state.</param>
    /// <param name="igdbSlug">The IGDB slug to link the game to, or <see langword="null" /> to unlink it.</param>
    /// <param name="platforms">The platforms the game has been played on.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated game, or an error if no game with the specified ID was
    ///     found or the slug is already linked to another game.
    /// </returns>
    public Result<Playable> UpdatePlayable(Guid id, string title, PlayableState state, string? igdbSlug,
        IEnumerable<GamePlatform> platforms)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var playable = context.Playables.Find(id);
        if (playable is null)
        {
            return Result.Fail($"No game with ID '{id}' was found.");
        }

        if (FindSlugConflict(context, igdbSlug, id) is { } conflict)
        {
            return Result.Fail(conflict);
        }

        playable.Title = title;
        playable.State = state;
        playable.IgdbSlug = igdbSlug;
        playable.Platforms = Normalize(platforms);
        context.SaveChanges();
        return Result.Ok(playable);
    }

    /// <summary>
    ///     Sets the state of a game.
    /// </summary>
    /// <param name="id">The ID of the game to update.</param>
    /// <param name="state">The new state.</param>
    /// <returns>A <see cref="Result" /> indicating success, or an error if no game with the specified ID was found.</returns>
    public Result SetState(Guid id, PlayableState state)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var playable = context.Playables.Find(id);
        if (playable is null)
        {
            return Result.Fail($"No game with ID '{id}' was found.");
        }

        playable.State = state;
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Removes a game from the game list.
    /// </summary>
    /// <param name="id">The ID of the game to remove.</param>
    /// <returns>A <see cref="Result" /> indicating success, or an error if no game with the specified ID was found.</returns>
    public Result DeletePlayable(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var playable = context.Playables.Find(id);
        if (playable is null)
        {
            return Result.Fail($"No game with ID '{id}' was found.");
        }

        context.Playables.Remove(playable);
        context.SaveChanges();
        return Result.Ok();
    }

    private static List<GamePlatform> Normalize(IEnumerable<GamePlatform> platforms)
    {
        return platforms.Where(platform => Enum.IsDefined(platform)).Distinct().Order().ToList();
    }

    private static string? FindSlugConflict(AppDbContext context, string? igdbSlug, Guid? exceptId)
    {
        if (igdbSlug is null)
        {
            return null;
        }

        var other = context.Playables.FirstOrDefault(p => p.IgdbSlug == igdbSlug && p.Id != exceptId);
        return other is null ? null : $"'{other.Title}' is already linked to that IGDB game.";
    }
}
