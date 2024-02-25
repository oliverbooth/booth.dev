namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a user which can log in to the blog.
/// </summary>
public interface IUser
{
    /// <summary>
    ///     Gets the URL of the user's avatar.
    /// </summary>
    /// <value>The URL of the user's avatar.</value>
    Uri AvatarUrl { get; }

    /// <summary>
    ///     Gets the email address of the user.
    /// </summary>
    /// <value>The email address of the user.</value>
    string EmailAddress { get; }

    /// <summary>
    ///     Gets the display name of the author.
    /// </summary>
    /// <value>The display name of the author.</value>
    string DisplayName { get; }

    /// <summary>
    ///     Gets the unique identifier of the user.
    /// </summary>
    /// <value>The unique identifier of the user.</value>
    Guid Id { get; }

    /// <summary>
    ///     Gets the date and time the user registered.
    /// </summary>
    /// <value>The registration date and time.</value>
    DateTimeOffset Registered { get; }

    /// <summary>
    ///     Gets the permissions this user is granted.
    /// </summary>
    /// <value>A read-only view of the permissions this user is granted.</value>
    IReadOnlyList<Permission> Permissions { get; }

    /// <summary>
    ///     Gets the user's TOTP token.
    /// </summary>
    /// <value>The TOTP token.</value>
    string? Totp { get; }

    /// <summary>
    ///     Gets the URL of the user's avatar.
    /// </summary>
    /// <param name="size">The size of the avatar.</param>
    /// <returns>The URL of the user's avatar.</returns>
    Uri GetAvatarUrl(int size = 28);

    /// <summary>
    ///     Determines whether the user has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to test.</param>
    /// <returns>
    ///     <see langword="true" /> if the user has the specified permission; otherwise, <see langword="false" />.
    /// </returns>
    bool HasPermission(Permission permission);

    /// <summary>
    ///     Determines whether the user has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to test.</param>
    /// <returns>
    ///     <see langword="true" /> if the user has the specified permission; otherwise, <see langword="false" />.
    /// </returns>
    bool HasPermission(string permission);

    /// <summary>
    ///     Returns a value indicating whether the specified password is valid for the user.
    /// </summary>
    /// <param name="password">The password to test.</param>
    /// <returns>
    ///     <see langword="true" /> if the specified password is valid for the user; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    bool TestCredentials(string password);
}
