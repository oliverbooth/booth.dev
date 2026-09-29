using System.Collections.Concurrent;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown.Link;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OtpNet;

namespace BoothDotDev.Services;

using BCrypt = BCrypt.Net.BCrypt;

/// <summary>
///     Represents a service for managing users.
/// </summary>
public sealed class UserService
{
    /// <summary>
    ///     The CDN area a user's uploaded avatar lives under.
    /// </summary>
    private const string AvatarArea = "users";

    // allow for a 1-step window before and after the current time step to account for clock drift
    private static readonly VerificationWindow TotpVerificationWindow = new(1, 1);

    private readonly string _cdnBaseUrl;
    private readonly CdnMediaService _cdnMediaService;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ConcurrentDictionary<Guid, User> _userCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="AppDbContext" />.
    /// </param>
    /// <param name="cdnMediaService">The <see cref="CdnMediaService" />.</param>
    /// <param name="cdnOptions">The CDN options.</param>
    public UserService(IDbContextFactory<AppDbContext> dbContextFactory, CdnMediaService cdnMediaService,
        IOptions<CdnOptions> cdnOptions)
    {
        _dbContextFactory = dbContextFactory;
        _cdnMediaService = cdnMediaService;
        _cdnBaseUrl = cdnOptions.Value.BaseUrl;
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
            TotpSecret = string.IsNullOrWhiteSpace(request.TotpSecret) ? null : request.TotpSecret,
            UseGravatar = request.UseGravatar
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
        user.UseGravatar = request.UseGravatar;
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
    ///     Resolves the URL of a user's avatar.
    /// </summary>
    /// <param name="user">The user whose avatar to resolve.</param>
    /// <param name="size">The size of the avatar, only meaningful for a Gravatar.</param>
    /// <returns>
    ///     <see cref="User.GetGravatarUrl" /> if <see cref="User.UseGravatar" /> is <see langword="true" />; otherwise, the CDN
    ///     URL of <see cref="User.AvatarFileName" />, or <see langword="null" /> if neither applies, so the caller falls back
    ///     to showing the user's initial.
    /// </returns>
    public Uri? GetAvatarUrl(User user, int size)
    {
        return user.UseGravatar ? user.GetGravatarUrl(size) : GetCustomAvatarUrl(user);
    }

    /// <summary>
    ///     Resolves the CDN URL of a user's uploaded custom avatar, regardless of <see cref="User.UseGravatar" />.
    /// </summary>
    /// <param name="user">The user whose custom avatar to resolve.</param>
    /// <returns>The CDN URL of <see cref="User.AvatarFileName" />, or <see langword="null" /> if none has been uploaded.</returns>
    public Uri? GetCustomAvatarUrl(User user)
    {
        return user.AvatarFileName is { } fileName
            ? new Uri(CdnMediaResolver.BuildCdnUrl(_cdnBaseUrl, AvatarArea, MediaKind.Image, user.Registered, user.Id, fileName))
            : null;
    }

    /// <summary>
    ///     Uploads a new custom avatar for a user, replacing any previous one.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="file">The uploaded image.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the avatar could not be uploaded.</returns>
    public async Task<Result> SetAvatarAsync(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = context.Users.Find(id);
        if (user is null)
        {
            return Result.Fail($"User with ID '{id}' not found.");
        }

        if (CdnMediaResolver.ResolveMediaKind(file.FileName) != MediaKind.Image)
        {
            return Result.Fail($"'{file.FileName}' isn't an image.");
        }

        // replacing the avatar with a file of the same name as the current one would otherwise collide with itself,
        // since the old file is normally only removed once the new upload has already succeeded
        if (user.AvatarFileName is { } currentFileName &&
            string.Equals(Path.GetFileName(file.FileName), currentFileName, StringComparison.OrdinalIgnoreCase))
        {
            _cdnMediaService.DeleteFile(id, user.Registered, currentFileName, AvatarArea);
        }

        var uploadResult = await _cdnMediaService.UploadAsync(id, user.Registered, file, AvatarArea, cancellationToken);
        if (uploadResult.IsFailed)
        {
            return uploadResult.ToResult();
        }

        if (user.AvatarFileName is { } oldFileName && oldFileName != uploadResult.Value.FileName)
        {
            _cdnMediaService.DeleteFile(id, user.Registered, oldFileName, AvatarArea);
        }

        user.AvatarFileName = uploadResult.Value.FileName;
        context.SaveChanges();

        _userCache[id] = user;
        return Result.Ok();
    }

    /// <summary>
    ///     Clears a user's custom avatar, deleting it from the CDN.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the avatar could not be cleared.</returns>
    public Result ClearAvatar(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var user = context.Users.Find(id);
        if (user is null)
        {
            return Result.Fail($"User with ID '{id}' not found.");
        }

        if (user.AvatarFileName is { } fileName)
        {
            _cdnMediaService.DeleteFile(id, user.Registered, fileName, AvatarArea);
        }

        user.AvatarFileName = null;
        context.SaveChanges();

        _userCache[id] = user;
        return Result.Ok();
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
