using System.Net.Http.Headers;
using System.Text.Json;
using BoothDotDev.Data;
using FluentResults;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for pulling the watchlist and watched status from Trakt and reconciling them against the
///     site's own watchlist.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient" /> to use for making requests to Trakt.</param>
/// <param name="traktAuthService">The Trakt auth service.</param>
/// <param name="watchlistService">The watchlist service.</param>
/// <param name="options">The Trakt options.</param>
/// <param name="logger">The logger.</param>
public sealed class TraktSyncService(
    HttpClient httpClient,
    TraktAuthService traktAuthService,
    WatchlistService watchlistService,
    IOptionsMonitor<TraktOptions> options,
    ILogger<TraktSyncService> logger)
{
    private const string ApiBaseUrl = "https://api.trakt.tv";

    /// <summary>
    ///     Pulls the current watchlist and watched status from Trakt and reconciles them against the site's own
    ///     watchlist. A show only counts as watched once every non-special episode does - a partially-watched show
    ///     is left wherever it already is.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing a summary of the changes made.</returns>
    public async Task<Result<WatchlistSyncSummary>> SyncAsync(CancellationToken cancellationToken)
    {
        var tokenResult = await traktAuthService.GetValidAccessTokenAsync(cancellationToken);
        if (tokenResult.IsFailed)
        {
            return tokenResult.ToResult();
        }

        var accessToken = tokenResult.Value;

        var watchlistResult = await GetMediaRefsAsync(accessToken, "/sync/watchlist", cancellationToken);
        if (watchlistResult.IsFailed)
        {
            return watchlistResult.ToResult();
        }

        var watchedMoviesResult =
            await GetMediaRefsAsync(accessToken, "/sync/watched/movies", cancellationToken, WatchableKind.Movie);
        if (watchedMoviesResult.IsFailed)
        {
            return watchedMoviesResult.ToResult();
        }

        var watchedShowCandidatesResult =
            await GetMediaRefsAsync(accessToken, "/sync/watched/shows", cancellationToken, WatchableKind.Show);
        if (watchedShowCandidatesResult.IsFailed)
        {
            return watchedShowCandidatesResult.ToResult();
        }

        var fullyWatchedShows = new List<TraktMediaRef>();
        foreach (var candidate in watchedShowCandidatesResult.Value)
        {
            var progressResult = await GetShowProgressAsync(accessToken, candidate.TraktId, cancellationToken);
            if (progressResult.IsFailed)
            {
                // one show's progress check failing isn't worth aborting the whole sync over.
                // leave at whatever was already on the site, until it gets another chance next time.
                logger.LogWarning("Couldn't check watched progress for Trakt show {TraktId}: {Error}",
                    candidate.TraktId, progressResult.Errors[0].Message);
                continue;
            }

            var (aired, completed) = progressResult.Value;
            if (aired > 0 && completed >= aired)
            {
                fullyWatchedShows.Add(candidate);
            }
        }

        var watched = watchedMoviesResult.Value.Concat(fullyWatchedShows).ToArray();
        var summary = watchlistService.ReconcileFromTrakt(watchlistResult.Value, watched);
        return Result.Ok(summary);
    }

    /// <summary>
    ///     Gets a show's watched progress, excluding specials.
    /// </summary>
    /// <param name="accessToken">The access token to authenticate with.</param>
    /// <param name="traktShowId">The Trakt ID of the show.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the number of aired and completed non-special episodes.</returns>
    private async Task<Result<(int Aired, int Completed)>> GetShowProgressAsync(
        string accessToken, int traktShowId, CancellationToken cancellationToken)
    {
        using var response =
            await SendAsync(accessToken, $"/shows/{traktShowId}/progress/watched?specials=false", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail($"Trakt returned {(int)response.StatusCode} for show {traktShowId}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var aired = document.RootElement.GetProperty("aired").GetInt32();
        var completed = document.RootElement.GetProperty("completed").GetInt32();
        return Result.Ok((aired, completed));
    }

    /// <summary>
    ///     Gets and parses a list of movie/show references from a Trakt endpoint. Used for the watchlist and both
    ///     watched-movies and watched-shows endpoints, all of which share the same
    ///     <c>{type, movie: {...}}</c>/<c>{type, show: {...}}</c> item shape.
    /// </summary>
    /// <param name="accessToken">The access token to authenticate with.</param>
    /// <param name="path">The API path to request.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <param name="forcedKind">
    ///     The kind to assume for every item, for an endpoint that's single-kind by URL (e.g. <c>/sync/watched/movies</c>).
    ///     If <see langword="null" />, each item's kind is read from its own <c>type</c> field instead, for an endpoint
    ///     that mixes both (e.g. <c>/sync/watchlist</c>).
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the parsed references.</returns>
    private async Task<Result<IReadOnlyList<TraktMediaRef>>> GetMediaRefsAsync(
        string accessToken, string path, CancellationToken cancellationToken, WatchableKind? forcedKind = null)
    {
        using var response = await SendAsync(accessToken, path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Fail($"Trakt returned {(int)response.StatusCode} for {path}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var refs = new List<TraktMediaRef>();
        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (ParseMediaRef(item, forcedKind) is { } mediaRef)
            {
                refs.Add(mediaRef);
            }
        }

        return Result.Ok<IReadOnlyList<TraktMediaRef>>(refs);
    }

    /// <summary>
    ///     Parses a single movie/show reference from a Trakt list item, e.g. <c>{"type": "movie", "movie": {...}}</c>.
    /// </summary>
    /// <param name="item">The item to parse.</param>
    /// <param name="forcedKind">The kind to assume, bypassing the item's own <c>type</c> field if present.</param>
    /// <returns>The parsed reference, or <see langword="null" /> if the item isn't a recognized movie or show.</returns>
    private static TraktMediaRef? ParseMediaRef(JsonElement item, WatchableKind? forcedKind)
    {
        WatchableKind resolvedKind;
        if (forcedKind is { } kind)
        {
            resolvedKind = kind;
        }
        else
        {
            var type = item.TryGetProperty("type", out var typeProperty) ? typeProperty.GetString() : null;
            var parsedKind = type switch
            {
                "movie" => WatchableKind.Movie,
                "show" => WatchableKind.Show,
                _ => (WatchableKind?)null
            };

            if (parsedKind is not { } k)
            {
                return null;
            }

            resolvedKind = k;
        }

        var mediaKey = resolvedKind == WatchableKind.Movie ? "movie" : "show";
        if (!item.TryGetProperty(mediaKey, out var media) ||
            media.GetProperty("title").GetString() is not { Length: > 0 } title)
        {
            return null;
        }

        var traktId = media.GetProperty("ids").GetProperty("trakt").GetInt32();
        return new TraktMediaRef(traktId, resolvedKind, title);
    }

    /// <summary>
    ///     Sends an authenticated GET request to the Trakt API.
    /// </summary>
    /// <param name="accessToken">The access token to authenticate with.</param>
    /// <param name="path">The API path to request, including any query string.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The response.</returns>
    private async Task<HttpResponseMessage> SendAsync(string accessToken, string path, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBaseUrl}{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("trakt-api-key", options.CurrentValue.ClientId);
        request.Headers.Add("trakt-api-version", "2");

        // required by Trakt - api.trakt.tv (unlike auth.trakt.tv) rejects requests without user agent, returning a 403 that looks
        // identical to a bad API key
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("booth.dev", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return await httpClient.SendAsync(request, cancellationToken);
    }
}
