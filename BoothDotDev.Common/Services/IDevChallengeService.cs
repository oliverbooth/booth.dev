using BoothDotDev.Common.Data.Web;

namespace BoothDotDev.Common.Services;

/// <summary>
///     Represents the service which fetches and manages the dev challenges.
/// </summary>
public interface IDevChallengeService
{
    /// <summary>
    ///     Gets a read-only collection of dev challenges.
    /// </summary>
    /// <returns>A read-only collection of dev challenges.</returns>
    IReadOnlyList<IDevChallenge> GetDevChallenges();

    /// <summary>
    ///     Gets a read-only collection of dev challenges.
    /// </summary>
    /// <param name="id">The id of the dev challenge.</param>
    /// <param name="devChallenge">
    ///     When this method returns, contains the dev challenge associated with the specified id, if the id is found;
    ///     otherwise, the default value for the type will be returned. This parameter is passed uninitialized.
    /// </param>
    /// <returns>A read-only collection of dev challenges.</returns>
    bool TryGetDevChallenge(int id, out IDevChallenge? devChallenge);
}
