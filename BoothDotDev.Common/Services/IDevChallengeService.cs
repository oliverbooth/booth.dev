using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Web;

namespace BoothDotDev.Common.Services;

/// <summary>
///     Represents the service which fetches and manages the dev challenges.
/// </summary>
public interface IDevChallengeService
{
    /// <summary>
    ///     Authenticates the challenge with the specified ID and password.
    /// </summary>
    /// <param name="id">The ID of the challenge.</param>
    /// <param name="password">The password of the challenge.</param>
    /// <returns><see langword="true" /> if the challenge is authenticated; otherwise, <see langword="false" />.</returns>
    bool AuthenticateChallenge(string id, string? password);

    /// <summary>
    ///     Gets a read-only collection of dev challenges.
    /// </summary>
    /// <returns>A read-only collection of dev challenges.</returns>
    IReadOnlyList<IDevChallenge> GetDevChallenges(Visibility visibility = Visibility.None);

    /// <summary>
    ///     Gets a read-only collection of dev challenges.
    /// </summary>
    /// <param name="id">The id of the dev challenge.</param>
    /// <param name="devChallenge">
    ///     When this method returns, contains the dev challenge associated with the specified id, if the id is found;
    ///     otherwise, the default value for the type will be returned. This parameter is passed uninitialized.
    /// </param>
    /// <param name="shouldRedirect">
    ///     When this method returns, contains a value indicating whether the user should be redirected to the new URL.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns>A read-only collection of dev challenges.</returns>
    bool TryGetDevChallenge(string id, [NotNullWhen(true)] out IDevChallenge? devChallenge, out bool shouldRedirect);
}
