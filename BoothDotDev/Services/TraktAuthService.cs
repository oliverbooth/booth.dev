using System.Net;
using System.Text.Json.Serialization;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for authenticating with Trakt via its device code OAuth flow, and keeping the resulting
///     token pair fresh.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient" /> to use for making requests to Trakt.</param>
/// <param name="dbContextFactory">The database context factory.</param>
/// <param name="cache">The <see cref="IMemoryCache" /> to use for holding the in-progress device code between requests.</param>
/// <param name="options">The Trakt options.</param>
public sealed class TraktAuthService(
    HttpClient httpClient,
    IDbContextFactory<AppDbContext> dbContextFactory,
    IMemoryCache cache,
    IOptionsMonitor<TraktOptions> options)
{
    private const string AuthBaseUrl = "https://auth.trakt.tv";
    private const string PendingDeviceCodeCacheKey = "trakt_pending_device_code";
    private const string RedirectUri = "urn:ietf:wg:oauth:2.0:oob";

    /// <summary>
    ///     Gets a value indicating whether Trakt has been connected.
    /// </summary>
    /// <returns><see langword="true" /> if a credential has been stored; otherwise, <see langword="false" />.</returns>
    public bool IsConnected()
    {
        using var context = dbContextFactory.CreateDbContext();
        return context.TraktCredentials.Any(c => c.Id == TraktCredential.SingletonId);
    }

    /// <summary>
    ///     Starts the device code flow.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the challenge to present to the user.</returns>
    public async Task<Result<TraktDeviceChallenge>> StartDeviceAuthAsync(CancellationToken cancellationToken)
    {
        var clientId = options.CurrentValue.ClientId;
        if (string.IsNullOrEmpty(clientId))
        {
            return Result.Fail("Trakt isn't configured. Add a client ID and secret to config.yaml first.");
        }

        using var response = await httpClient.PostAsJsonAsync($"{AuthBaseUrl}/oauth/device/code",
            new { client_id = clientId }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Couldn't start the Trakt device authorization flow.");
        }

        var body = await response.Content.ReadFromJsonAsync<DeviceCodeResponse>(cancellationToken);
        if (body is null)
        {
            return Result.Fail("Trakt returned an unexpected response.");
        }

        var challenge = new TraktDeviceChallenge(body.UserCode, body.VerificationUrl, body.ExpiresIn);
        cache.Set(PendingDeviceCodeCacheKey, new PendingDeviceAuth(body.DeviceCode, challenge),
            TimeSpan.FromSeconds(body.ExpiresIn));

        return Result.Ok(challenge);
    }

    /// <summary>
    ///     Gets the currently pending device authorization challenge, if a "Connect Trakt" flow was started and
    ///     hasn't yet completed, been denied, or expired.
    /// </summary>
    /// <returns>The pending challenge, or <see langword="null" /> if there isn't one.</returns>
    public TraktDeviceChallenge? GetPendingChallenge()
    {
        return cache.TryGetValue(PendingDeviceCodeCacheKey, out PendingDeviceAuth? pending) ? pending?.Challenge : null;
    }

    /// <summary>
    ///     Checks whether the user has authorized the pending device code, storing the resulting token pair if so.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result" /> indicating whether authorization completed.</returns>
    public async Task<Result> CheckDeviceAuthAsync(CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(PendingDeviceCodeCacheKey, out PendingDeviceAuth? pending) || pending is null)
        {
            return Result.Fail("No authorization is in progress. Start over.");
        }

        var deviceCode = pending.DeviceCode;

        var clientId = options.CurrentValue.ClientId;
        var clientSecret = options.CurrentValue.ClientSecret;

        using var response = await httpClient.PostAsJsonAsync($"{AuthBaseUrl}/oauth/device/token",
            new { code = deviceCode, client_id = clientId, client_secret = clientSecret }, cancellationToken);

        // https://docs.trakt.tv/reference/postoauthdevicetoken - 400 means "still waiting", everything else is
        // either success or a reason to give up and restart the flow.
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return Result.Fail("Not authorized yet - visit the link, enter the code, then check again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            cache.Remove(PendingDeviceCodeCacheKey);
            var reason = response.StatusCode switch
            {
                HttpStatusCode.Gone => "The code expired.",
                HttpStatusCode.Conflict => "That code was already used.",
                (HttpStatusCode)418 => "Authorization was denied.",
                HttpStatusCode.TooManyRequests => "Checked too soon - wait a moment and try again.",
                _ => "Authorization failed."
            };
            return Result.Fail($"{reason} Start over.");
        }

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        if (body is null)
        {
            return Result.Fail("Trakt returned an unexpected response.");
        }

        SaveToken(body);
        cache.Remove(PendingDeviceCodeCacheKey);
        return Result.Ok();
    }

    /// <summary>
    ///     Gets a valid access token, refreshing the stored one first if it's expired or close to it.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing a valid access token.</returns>
    public async Task<Result<string>> GetValidAccessTokenAsync(CancellationToken cancellationToken)
    {
        TraktCredential? credential;
        await using (var context = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            credential = await context.TraktCredentials.FindAsync([TraktCredential.SingletonId], cancellationToken);
        }

        if (credential is null)
        {
            return Result.Fail("Trakt isn't connected yet.");
        }

        // refreshed a little ahead of the real expiry so an in-flight sync never gets caught out by it lapsing mid-request
        if (credential.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            return Result.Ok(credential.AccessToken);
        }

        var clientId = options.CurrentValue.ClientId;
        var clientSecret = options.CurrentValue.ClientSecret;

        using var response = await httpClient.PostAsJsonAsync($"{AuthBaseUrl}/oauth/token",
            new
            {
                client_id = clientId,
                client_secret = clientSecret,
                refresh_token = credential.RefreshToken,
                redirect_uri = RedirectUri,
                grant_type = "refresh_token"
            }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail("Couldn't refresh the Trakt token. Reconnect Trakt.");
        }

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        if (body is null)
        {
            return Result.Fail("Trakt returned an unexpected response.");
        }

        SaveToken(body);
        return Result.Ok(body.AccessToken);
    }

    /// <summary>
    ///     Saves a token response as the current credential, replacing whatever was there before.
    /// </summary>
    /// <param name="token">The token response to save.</param>
    private void SaveToken(TokenResponse token)
    {
        using var context = dbContextFactory.CreateDbContext();
        var credential = context.TraktCredentials.Find(TraktCredential.SingletonId) ??
                         context.TraktCredentials.Add(new TraktCredential { Id = TraktCredential.SingletonId }).Entity;

        credential.AccessToken = token.AccessToken;
        credential.RefreshToken = token.RefreshToken;
        credential.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
        context.SaveChanges();
    }

    private sealed record PendingDeviceAuth(string DeviceCode, TraktDeviceChallenge Challenge);

    private sealed record DeviceCodeResponse(
        [property: JsonPropertyName("device_code")]
        string DeviceCode,
        [property: JsonPropertyName("user_code")]
        string UserCode,
        [property: JsonPropertyName("verification_url")]
        string VerificationUrl,
        [property: JsonPropertyName("expires_in")]
        int ExpiresIn,
        [property: JsonPropertyName("interval")]
        int Interval);

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")]
        string AccessToken,
        [property: JsonPropertyName("refresh_token")]
        string RefreshToken,
        [property: JsonPropertyName("expires_in")]
        int ExpiresIn);
}

/// <summary>
///     Represents the challenge presented to the user during the Trakt device code flow.
/// </summary>
/// <param name="UserCode">The code the user must enter at <paramref name="VerificationUrl" />.</param>
/// <param name="VerificationUrl">The URL the user must visit to enter the code.</param>
/// <param name="ExpiresInSeconds">How long, in seconds, the code remains valid for.</param>
public sealed record TraktDeviceChallenge(string UserCode, string VerificationUrl, int ExpiresInSeconds);
