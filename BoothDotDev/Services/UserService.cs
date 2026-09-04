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
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.Users.OrderBy(u => u.DisplayName)];
    }

    /// <summary>
    ///     Creates a new user.
    /// </summary>
    /// <param name="request">The user's display name, email address, and login state.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created user.</returns>
    public Result<User> CreateUser(UserSaveRequest request)
    {
        if (!request.DisableLogin && string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Result.Fail("A password is required unless login is disabled.");
        }

        using var context = _dbContextFactory.CreateDbContext();
        var user = new User
        {
            DisplayName = request.DisplayName,
            EmailAddress = request.EmailAddress,
            TotpSecret = string.IsNullOrWhiteSpace(request.TotpSecret) ? null : request.TotpSecret
        };

        ApplyPassword(user, request);

        context.Users.Add(user);
        context.SaveChanges();

        _userCache[user.Id] = user;
        return user;
    }

    /// <summary>
    ///     Updates an existing user.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="request">The user's display name, email address, and login state.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated user, or an error if no user with the specified
    ///     <paramref name="id" /> exists, or if login would end up enabled with no password set.
    /// </returns>
    public Result<User> UpdateUser(Guid id, UserSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = context.Users.Find(id);

        if (user is null)
        {
            return Result.Fail($"User with ID '{id}' not found.");
        }

        // a blank password field during an edit means "leave it unchanged" - but only if there's an existing
        // password to fall back to. A user with login already disabled has nothing to fall back to, so leaving
        // it blank while also unchecking "disable login" would silently leave login disabled anyway.
        var hasExistingPassword = !string.IsNullOrWhiteSpace(user.Password);
        if (!request.DisableLogin && string.IsNullOrWhiteSpace(request.NewPassword) && !hasExistingPassword)
        {
            return Result.Fail("A password is required to enable login.");
        }

        user.DisplayName = request.DisplayName;
        user.EmailAddress = request.EmailAddress;
        user.TotpSecret = string.IsNullOrWhiteSpace(request.TotpSecret) ? null : request.TotpSecret;
        ApplyPassword(user, request);

        context.SaveChanges();

        _userCache[id] = user;
        return user;
    }

    /// <summary>
    ///     Generates a new random TOTP secret, base32-encoded and ready to hand to an authenticator app.
    /// </summary>
    /// <returns>A new random TOTP secret.</returns>
    public static string GenerateTotpSecret()
    {
        return Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
    }

    /// <summary>
    ///     Resets a user's TOTP, clearing their secret so they're no longer prompted for a code at login. There's
    ///     no self-service re-enrollment flow - a new secret has to be configured directly in the database, same as
    ///     the initial setup.
    /// </summary>
    /// <param name="id">The ID of the user whose TOTP to reset.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated user, or an error if no user with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<User> ResetTotp(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = context.Users.Find(id);

        if (user is null)
        {
            return Result.Fail($"User with ID '{id}' not found.");
        }

        user.TotpSecret = null;
        context.SaveChanges();

        _userCache[id] = user;
        return user;
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

        using var context = _dbContextFactory.CreateDbContext();
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

        using var context = _dbContextFactory.CreateDbContext();
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

    /// <summary>
    ///     Applies a save request's login state to a user: clears the password if login is being disabled, hashes
    ///     a new password if one was given, or leaves the existing password untouched otherwise.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="request">The save request.</param>
    private static void ApplyPassword(User user, UserSaveRequest request)
    {
        if (request.DisableLogin)
        {
            user.Password = string.Empty;
            user.Salt = string.Empty;
            return;
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return;
        }

        user.Password = BCrypt.HashPassword(request.NewPassword);
        user.Salt = BCrypt.GenerateSalt();
    }
}
