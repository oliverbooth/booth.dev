using System.Collections.Concurrent;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace BoothDotDev.Services;

using BCrypt = BCrypt.Net.BCrypt;

/// <summary>
///     Represents a service for managing users.
/// </summary>
public sealed class UserService
{
    // allow for a 1-step window before and after the current time step to account for clock drift
    private static readonly VerificationWindow TotpVerificationWindow = new(1, 1);

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ConcurrentDictionary<Guid, User> _userCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="AppDbContext" />.
    /// </param>
    public UserService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Returns a read-only view of all users.
    /// </summary>
    /// <returns>A read-only view of all users, ordered by display name.</returns>
    public IReadOnlyList<User> GetAllUsers()
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.Users.OrderBy(u => u.DisplayName)];
    }

    /// <summary>
    ///     Finds a user with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the user to find.</param>
    /// <returns>A <see cref="Result{T}" /> containing the user if found; otherwise, an error result.</returns>
    public Result<User> GetUser(Guid id)
    {
        if (_userCache.TryGetValue(id, out var user))
        {
            return user;
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.Find(id);

        if (user is not null)
        {
            _userCache.TryAdd(id, user);
        }

        return user is not null ? Result.Ok(user) : Result.Fail("User not found.");
    }

    /// <summary>
    ///     Verifies the password for a user with the specified email address.
    /// </summary>
    /// <param name="email">The email address of the user to verify.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>A <see cref="Result{T}" /> containing the user if the password is valid; otherwise, an error result.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="email" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="password" /> is <see langword="null" />.</para>
    /// </exception>
    public Result<User> VerifyPassword(string email, string password)
    {
        if (email is null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        if (password is null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var user = context.Users.FirstOrDefault(u => u.EmailAddress == email);

        if (user is null)
        {
            return Result.Fail("Invalid email or password.");
        }

        if (string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Salt))
        {
            return Result.Fail("Invalid email or password.");
        }

        if (BCrypt.Verify(password, user.Password))
        {
            return Result.Ok(user);
        }

        return Result.Fail("Invalid email or password.");
    }

    /// <summary>
    ///     Verifies the TOTP for a user with the specified ID.
    /// </summary>
    /// <param name="userId">The ID of the user to verify.</param>
    /// <param name="totpCode">The TOTP code to verify.</param>
    /// <returns>A <see cref="Result{T}" /> containing the user if the TOTP is valid; otherwise, an error result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="totpCode" /> is <see langword="null" />.</exception>
    public Result<User> VerifyTotp(Guid userId, string totpCode)
    {
        if (totpCode is null)
        {
            throw new ArgumentNullException(nameof(totpCode));
        }

        var result = GetUser(userId);
        if (result.IsFailed)
        {
            return Result.Fail("User not found.");
        }

        var user = result.Value;

        if (user.TotpSecret is null)
        {
            return Result.Fail("TOTP is not configured for this account.");
        }

        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));

        if (!totp.VerifyTotp(totpCode, out _, TotpVerificationWindow))
        {
            return Result.Fail("Invalid TOTP code.");
        }

        return Result.Ok(user);
    }
}
