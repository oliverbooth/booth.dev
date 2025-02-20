using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using BoothDotDev.Data.Web;
using Microsoft.EntityFrameworkCore;

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
    public IReadOnlyList<IDevChallenge> GetDevChallenges()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.DevChallenges.OrderBy(c => c.Date).ToArray<IDevChallenge>();
    }

    /// <inheritdoc />
    public bool TryGetDevChallenge(int id, out IDevChallenge? devChallenge)
    {
        using var context = _dbContextFactory.CreateDbContext();
        devChallenge = context.DevChallenges.Find(id);
        return devChallenge is not null;
    }
}
