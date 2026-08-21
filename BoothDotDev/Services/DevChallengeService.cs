using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using DEDrake;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which fetches and manages dev challenges.
/// </summary>
public sealed class DevChallengeService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DevChallengeService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The factory for creating the web database context.</param>
    public DevChallengeService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Authenticates the challenge with the specified ID and password.
    /// </summary>
    /// <param name="id">The ID of the challenge.</param>
    /// <param name="password">The password of the challenge.</param>
    /// <returns><see langword="true" /> if the challenge is authenticated; otherwise, <see langword="false" />.</returns>
    public bool AuthenticateChallenge(string id, string? password)
    {
        if (!TryGetDevChallenge(id, out var challenge, out _))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(challenge.Password))
        {
            return true;
        }

        return password is not null && BC.Verify(password, challenge.Password);
    }

    /// <summary>
    ///     Gets a read-only collection of dev challenges.
    /// </summary>
    /// <param name="visibility">The visibility of the dev challenges.</param>
    /// <returns>A read-only collection of dev challenges.</returns>
    public IReadOnlyList<DevChallenge> GetDevChallenges(Visibility visibility)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        IQueryable<DevChallenge> challenges = context.DevChallenges.OrderBy(c => c.PublishedAt);

        if (visibility != Visibility.None)
        {
            challenges = challenges.Where(c => c.Visibility == visibility);
        }

        return [.. challenges];
    }

    /// <summary>
    ///     Returns the most recent dev challenges, limited to the specified count.
    /// </summary>
    /// <param name="searchOptions">The options for searching and retrieving dev challenges.</param>
    /// <returns>A read-only list of the most recent dev challenges.</returns>
    public IReadOnlyList<DevChallenge> GetRecentChallenges(ActivitySearchOptions searchOptions)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var challenges = context.DevChallenges.AsQueryable();

        if (searchOptions.Visibility != Visibility.None)
        {
            challenges = challenges.Where(c => c.Visibility == searchOptions.Visibility);
        }

        var ordered = challenges.OrderByDescending(c => c.PublishedAt);
        return [.. ordered.Take(searchOptions.Count)];
    }

    /// <summary>
    ///     Tries to get a dev challenge by its ID.
    /// </summary>
    /// <param name="id">The ID of the dev challenge.</param>
    /// <param name="devChallenge">
    ///     When this method returns, contains the dev challenge associated with the specified id, if the id is found;
    ///     otherwise, the default value for the type will be returned. This parameter is passed uninitialized.
    /// </param>
    /// <param name="shouldRedirect">
    ///     When this method returns, contains a value indicating whether the user should be redirected to the new URL.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns><see langword="true" /> if the dev challenge is found; otherwise, <see langword="false" />.</returns>
    public bool TryGetDevChallenge(string id,
        [NotNullWhen(true)] out DevChallenge? devChallenge,
        out bool shouldRedirect)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            devChallenge = null;
            shouldRedirect = false;
            return false;
        }

        using var context = _dbContextFactory.CreateDbContext();
        if (int.TryParse(id, out var oldId))
        {
            devChallenge = context.DevChallenges.FirstOrDefault(c => c.OldId == oldId);
            shouldRedirect = devChallenge is not null;
            return shouldRedirect;
        }

        ShortGuid guid;

        try
        {
            guid = ShortGuid.Parse(id);
        }
        catch (FormatException)
        {
            devChallenge = null;
            shouldRedirect = false;
            return false;
        }

        devChallenge = context.DevChallenges.Find(guid);
        shouldRedirect = false;
        return devChallenge is not null;
    }
}
