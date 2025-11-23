using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using BoothDotDev.Data.Web;
using DEDrake;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace BoothDotDev.Services;

/// <inheritdoc />
internal sealed class DevChallengeService : IDevChallengeService
{
    private readonly IDbContextFactory<WebContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DevChallengeService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The factory for creating the web database context.</param>
    public DevChallengeService(IDbContextFactory<WebContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IReadOnlyList<IDevChallenge> GetDevChallenges(Visibility visibility)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        IQueryable<DevChallenge> challenges = context.DevChallenges.OrderBy(c => c.Date);

        if (visibility != Visibility.None)
        {
            challenges = challenges.Where(c => c.Visibility == visibility);
        }

        return challenges.ToArray();
    }

    /// <inheritdoc />
    public bool TryGetDevChallenge(string id,
        [NotNullWhen(true)] out IDevChallenge? devChallenge,
        out bool shouldRedirect)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            devChallenge = null;
            shouldRedirect = false;
            return false;
        }

        using var context = _dbContextFactory.CreateDbContext();
        if (int.TryParse(id, out int oldId))
        {
            devChallenge = context.DevChallenges.FirstOrDefault(c => c.OldId == oldId);
            shouldRedirect = devChallenge is not null;
            return devChallenge is not null;
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
