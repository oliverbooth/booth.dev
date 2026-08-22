using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Fido2NetLib;
using Fido2NetLib.Objects;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for registering and verifying WebAuthn passkey credentials.
/// </summary>
public sealed class PasskeyService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IFido2 _fido2;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PasskeyService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The factory for creating the web database context.</param>
    /// <param name="fido2">The <see cref="IFido2" /> instance to use for WebAuthn ceremonies.</param>
    public PasskeyService(IDbContextFactory<AppDbContext> dbContextFactory, IFido2 fido2)
    {
        _dbContextFactory = dbContextFactory;
        _fido2 = fido2;
    }

    /// <summary>
    ///     Begins a passkey registration ceremony for the specified user.
    /// </summary>
    /// <param name="user">The user to register a new passkey for.</param>
    /// <returns>The credential creation options to send to the browser.</returns>
    public CredentialCreateOptions BeginRegistration(User user)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var excludeCredentials = context.PasskeyCredentials
            .Where(c => c.UserId == user.Id)
            .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
            .ToList();

        var fidoUser = new Fido2User
        {
            Id = user.Id.ToByteArray(),
            Name = user.EmailAddress,
            DisplayName = user.DisplayName
        };

        return _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fidoUser,
            ExcludeCredentials = excludeCredentials,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                // usernameless login requires a resident (discoverable) key - preferred lets some
                // authenticators silently create a non-resident credential that registers fine but can't
                // participate in that flow, so this can't be anything less than Required
                ResidentKey = ResidentKeyRequirement.Required,
                UserVerification = UserVerificationRequirement.Preferred
            },
            AttestationPreference = AttestationConveyancePreference.None,
            PubKeyCredParams = PubKeyCredParam.Defaults
        });
    }

    /// <summary>
    ///     Completes a passkey registration ceremony, verifying the authenticator's response and persisting the
    ///     new credential.
    /// </summary>
    /// <param name="user">The user the credential is being registered for.</param>
    /// <param name="originalOptions">The options returned by <see cref="BeginRegistration" /> for this ceremony.</param>
    /// <param name="attestationResponse">The authenticator's attestation response.</param>
    /// <param name="nickname">A user-supplied label for the new credential.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-registered credential.</returns>
    public async Task<Result<PasskeyCredential>> CompleteRegistration(User user, CredentialCreateOptions originalOptions,
        AuthenticatorAttestationRawResponse attestationResponse, string? nickname)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        RegisteredPublicKeyCredential registered;
        try
        {
            registered = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = attestationResponse,
                OriginalOptions = originalOptions,
                IsCredentialIdUniqueToUserCallback = (parameters, _) =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    var isUnique = !context.PasskeyCredentials.Any(c => c.CredentialId == parameters.CredentialId);
                    return Task.FromResult(isUnique);
                }
            });
        }
        catch (Fido2VerificationException ex)
        {
            return Result.Fail($"Passkey registration failed: {ex.Message}");
        }

        var credential = new PasskeyCredential
        {
            UserId = user.Id,
            CredentialId = registered.Id,
            PublicKey = registered.PublicKey,
            AaGuid = registered.AaGuid,
            SignatureCounter = registered.SignCount,
            Transports = registered.Transports is { Length: > 0 }
                ? string.Join(',', registered.Transports)
                : null,
            Nickname = nickname
        };

        context.PasskeyCredentials.Add(credential);
        await context.SaveChangesAsync();

        return credential;
    }

    /// <summary>
    ///     Begins a usernameless passkey login ceremony.
    /// </summary>
    /// <returns>The assertion options to send to the browser.</returns>
    public AssertionOptions BeginLogin()
    {
        return _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = [],
            UserVerification = UserVerificationRequirement.Preferred
        });
    }

    /// <summary>
    ///     Completes a passkey login ceremony, verifying the authenticator's response against the stored
    ///     credential and returning the signed-in user.
    /// </summary>
    /// <param name="originalOptions">The options returned by <see cref="BeginLogin" /> for this ceremony.</param>
    /// <param name="assertionResponse">The authenticator's assertion response.</param>
    /// <returns>A <see cref="Result{T}" /> containing the user who signed in.</returns>
    public async Task<Result<User>> CompleteLogin(AssertionOptions originalOptions,
        AuthenticatorAssertionRawResponse assertionResponse)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var credential = context.PasskeyCredentials.FirstOrDefault(c => c.CredentialId == assertionResponse.RawId);
        if (credential is null)
        {
            return Result.Fail("This passkey is not registered.");
        }

        VerifyAssertionResult verified;
        try
        {
            verified = await _fido2.MakeAssertionAsync(new MakeAssertionParams
            {
                AssertionResponse = assertionResponse,
                OriginalOptions = originalOptions,
                StoredPublicKey = credential.PublicKey,
                StoredSignatureCounter = (uint)credential.SignatureCounter,
                IsUserHandleOwnerOfCredentialIdCallback = (parameters, _) =>
                {
                    var ownsCredential = parameters.UserHandle.SequenceEqual(credential.UserId.ToByteArray());
                    return Task.FromResult(ownsCredential);
                }
            });
        }
        catch (Fido2VerificationException ex)
        {
            return Result.Fail($"Passkey login failed: {ex.Message}");
        }

        var user = context.Users.Find(credential.UserId);
        if (user is null)
        {
            return Result.Fail("This passkey is not registered.");
        }

        // mirrors VerifyPassword's implicit "disabled" gate (an empty Password means login is disabled) - a
        // passkey must not be able to sign in to an account that's been disabled through the admin UI
        if (string.IsNullOrWhiteSpace(user.Password))
        {
            return Result.Fail("Login is disabled for this account.");
        }

        credential.SignatureCounter = verified.SignCount;
        credential.LastUsedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();

        return user;
    }

    /// <summary>
    ///     Returns a user's registered passkeys, newest first.
    /// </summary>
    /// <param name="userId">The ID of the user whose passkeys to return.</param>
    /// <returns>A read-only view of the user's passkeys.</returns>
    public IReadOnlyList<PasskeyCredential> ListCredentials(Guid userId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.PasskeyCredentials.Where(c => c.UserId == userId).OrderByDescending(c => c.CreatedAt)];
    }

    /// <summary>
    ///     Deletes a registered passkey.
    /// </summary>
    /// <param name="credentialId">The ID of the credential to delete.</param>
    /// <returns>A <see cref="Result" /> indicating whether the credential was deleted.</returns>
    public Result DeleteCredential(Guid credentialId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var credential = context.PasskeyCredentials.Find(credentialId);

        if (credential is null)
        {
            return Result.Fail($"Passkey with ID '{credentialId}' not found.");
        }

        context.PasskeyCredentials.Remove(credential);
        context.SaveChanges();

        return Result.Ok();
    }
}
