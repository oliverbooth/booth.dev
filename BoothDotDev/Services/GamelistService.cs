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
        return context.Playables.Include(p => p.Editions.OrderBy(e => e.Position)).Where(p => p.State == state).ToArray();
    }

    /// <summary>
    ///     Gets every game on the game list.
    /// </summary>
    /// <returns>A collection of every game.</returns>
    public IReadOnlyCollection<Playable> GetAllPlayables()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Playables.Include(p => p.Editions.OrderBy(e => e.Position)).OrderBy(p => p.Title).ToArray();
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
        var playable = context.Playables.Include(p => p.Editions.OrderBy(e => e.Position)).FirstOrDefault(p => p.Id == id);
        return playable is null ? Result.Fail($"No game with ID '{id}' was found.") : Result.Ok(playable);
    }

    /// <summary>
    ///     Adds a new game to the game list.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="state">The state.</param>
    /// <param name="igdbSlug">The IGDB slug to link the game to, if any.</param>
    /// <param name="platforms">The platforms the game has been played on.</param>
    /// <param name="editions">The additional editions of the game that have also been played.</param>
    /// <returns>A <see cref="Result{T}" /> containing the added game, or an error if a slug is already linked.</returns>
    public Result<Playable> AddPlayable(string title, PlayableState state, string? igdbSlug,
        IEnumerable<GamePlatform> platforms, IReadOnlyList<EditionInput> editions)
    {
        using var context = _dbContextFactory.CreateDbContext();
        if (FindSlugConflict(context, CollectSlugs(igdbSlug, editions), null) is { } conflict)
        {
            return Result.Fail(conflict);
        }

        var playable = new Playable
        {
            Id = Guid.NewGuid(),
            Title = title,
            State = state,
            IgdbSlug = igdbSlug,
            Platforms = Normalize(platforms),
            Editions = BuildEditions(editions)
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
    /// <param name="editions">The additional editions of the game that have also been played.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated game, or an error if no game with the specified ID was
    ///     found or the slug is already linked to another game.
    /// </returns>
    public Result<Playable> UpdatePlayable(Guid id, string title, PlayableState state, string? igdbSlug,
        IEnumerable<GamePlatform> platforms, IReadOnlyList<EditionInput> editions)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var playable = context.Playables.Include(p => p.Editions).FirstOrDefault(p => p.Id == id);
        if (playable is null)
        {
            return Result.Fail($"No game with ID '{id}' was found.");
        }

        if (FindSlugConflict(context, CollectSlugs(igdbSlug, editions), id) is { } conflict)
        {
            return Result.Fail(conflict);
        }

        playable.Title = title;
        playable.State = state;
        playable.IgdbSlug = igdbSlug;
        playable.Platforms = Normalize(platforms);
        playable.Editions.Clear();
        playable.Editions.AddRange(BuildEditions(editions));
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

    private static List<string> CollectSlugs(string? primary, IReadOnlyList<EditionInput> editions)
    {
        return editions.Select(e => e.IgdbSlug).Prepend(primary).OfType<string>().ToList();
    }

    private static List<PlayableEdition> BuildEditions(IReadOnlyList<EditionInput> editions)
    {
        return editions.Select((edition, position) => new PlayableEdition
        {
            Id = Guid.NewGuid(),
            Label = edition.Label,
            IgdbSlug = edition.IgdbSlug,
            Platforms = Normalize(edition.Platforms),
            Position = position
        }).ToList();
    }

    private static string? FindSlugConflict(AppDbContext context, List<string> slugs, Guid? exceptId)
    {
        if (slugs.Count != slugs.Distinct().Count())
        {
            return "The same IGDB game can't be linked more than once.";
        }

        if (slugs.Count == 0)
        {
            return null;
        }

        var other = context.Playables.FirstOrDefault(p => p.Id != exceptId &&
                                                          ((p.IgdbSlug != null && slugs.Contains(p.IgdbSlug)) ||
                                                           p.Editions.Any(e => e.IgdbSlug != null &&
                                                                               slugs.Contains(e.IgdbSlug))));
        return other is null ? null : $"'{other.Title}' is already linked to one of those IGDB games.";
    }
}

/// <summary>
///     Represents the details of an additional edition of a game, as submitted for saving.
/// </summary>
/// <param name="Label">The label of the edition.</param>
/// <param name="IgdbSlug">The IGDB slug of the edition, or <see langword="null" /> if it has none.</param>
/// <param name="Platforms">The platforms the edition has been played on.</param>
public sealed record EditionInput(string Label, string? IgdbSlug, IEnumerable<GamePlatform> Platforms);
