using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
using OliverBooth.Data.Blog;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service for managing blog users.
/// </summary>
public sealed class BlogUserService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogUserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public BlogUserService(IDbContextFactory<BlogContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Attempts to authenticate the user with the specified email address and password.
    /// </summary>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="password">The password.</param>
    /// <param name="user">
    ///     When this method returns, contains the user with the specified email address and password, if the user
    ///     exists; otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the authentication was successful; otherwise, <see langword="false" />.
    /// </returns>
    public bool TryAuthenticateUser(string? emailAddress, string? password, [NotNullWhen(true)] out User? user)
    {
        if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(password))
        {
            user = null;
            return false;
        }

        using BlogContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.FirstOrDefault(u => u.EmailAddress == emailAddress);
        if (user is null)
        {
            return false;
        }

        string hashedPassword = BC.HashPassword(password, user.Salt);
        return hashedPassword == user.Password;
    }

    /// <summary>
    ///     Attempts to retrieve the user with the specified user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="user">
    ///     When this method returns, contains the user with the specified user ID, if the user exists; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the user exists; otherwise, <see langword="false" />.</returns>
    public bool TryGetUser(Guid userId, [NotNullWhen(true)] out User? user)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.FirstOrDefault(u => u.Id == userId);
        return user is not null;
    }

    /// <summary>
    ///     Returns a value indicating whether the specified user requires a password reset.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    ///     <see langword="true" /> if the specified user requires a password reset; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool UserRequiresPasswordReset(User user)
    {
        return string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Salt);
    }
}
