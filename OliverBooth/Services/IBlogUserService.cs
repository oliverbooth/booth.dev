using System.Diagnostics.CodeAnalysis;
using OliverBooth.Data.Blog;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service for managing users.
/// </summary>
public interface IBlogUserService
{
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
}
