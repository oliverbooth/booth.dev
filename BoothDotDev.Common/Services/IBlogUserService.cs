using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Common.Data.Blog;
using Microsoft.AspNetCore.Http;

namespace BoothDotDev.Common.Services;

/// <summary>
///     Represents a service for managing users.
/// </summary>
public interface IBlogUserService
{
    /// <summary>
    ///     Signs in the specified user.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="user">The user to sign in.</param>
    /// <returns>A task that represents the asynchronous sign-in operation.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="httpContext" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="user" /> is <see langword="null" />.</para>
    /// </exception>
    Task SignInAsync(HttpContext httpContext, IUser user);

    /// <summary>
    ///     Attempts to find a user with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the user to find.</param>
    /// <param name="user">
    ///     When this method returns, contains the user with the specified ID, if the user is found; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a user with the specified ID is found; otherwise, <see langword="false" />.
    /// </returns>
    bool TryGetUser(Guid id, [NotNullWhen(true)] out IUser? user);

    /// <summary>
    ///     Attempts to find a user with the specified email address.
    /// </summary>
    /// <param name="email">The email address of the user to find.</param>
    /// <param name="user">
    ///     When this method returns, contains the user with the specified email address, if the user is found; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a user with the specified email address is found; otherwise, <see langword="false" />.
    /// </returns>
    bool TryGetUser(string email, [NotNullWhen(true)] out IUser? user);
}
