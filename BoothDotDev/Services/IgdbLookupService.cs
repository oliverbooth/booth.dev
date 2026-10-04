using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BoothDotDev.Data;
using FluentResults;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for looking up game metadata from the Internet Game Database (IGDB), to help fill in a
///     game list entry from just a title.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient" /> to use for making requests to the IGDB and Twitch APIs.</param>
/// <param name="options">The IGDB options.</param>
public sealed class IgdbLookupService(HttpClient httpClient, IOptionsMonitor<IgdbOptions> options)
{
    private const int MaxResults = 8;
    private const string SearchUrl = "https://api.igdb.com/v4/games";
    private const string TokenUrl = "https://id.twitch.tv/oauth2/token";

    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _token;
    private DateTimeOffset _tokenExpiry;

    /// <summary>
    ///     Searches IGDB for games matching the given title.
    /// </summary>
    /// <param name="query">The title to search for.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the matching candidates, or an error if none were found.</returns>
    public async Task<Result<IReadOnlyList<IgdbCandidate>>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        query = query.Trim();
        if (query.Length == 0)
        {
            return Result.Fail("Enter a title to search for.");
        }

        var config = options.CurrentValue;
        if (string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.ClientSecret))
        {
            return Result.Fail("IGDB isn't configured. Enter the details by hand instead.");
        }

        try
        {
            var token = await GetTokenAsync(config, cancellationToken);
            if (token is null)
            {
                return Result.Fail("Couldn't authenticate with IGDB. Enter the details by hand instead.");
            }

            var escaped = query.Replace("\\", "\\\\").Replace("\"", "\\\"");
            using var request = new HttpRequestMessage(HttpMethod.Post, SearchUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("Client-ID", config.ClientId);
            request.Content = new StringContent(
                $"search \"{escaped}\"; fields name,slug,first_release_date; limit {MaxResults};",
                Encoding.UTF8,
                "text/plain");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail("Couldn't reach IGDB. Enter the details by hand instead.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var candidates = new List<IgdbCandidate>();
            foreach (var game in document.RootElement.EnumerateArray())
            {
                if (!game.TryGetProperty("name", out var nameProperty) ||
                    nameProperty.GetString() is not { Length: > 0 } name ||
                    !game.TryGetProperty("slug", out var slugProperty) ||
                    slugProperty.GetString() is not { Length: > 0 } slug)
                {
                    continue;
                }

                var year = game.TryGetProperty("first_release_date", out var dateProperty) &&
                           dateProperty.TryGetInt64(out var seconds)
                    ? DateTimeOffset.FromUnixTimeSeconds(seconds).Year
                    : (int?)null;

                candidates.Add(new IgdbCandidate(name, slug, year));
            }

            return candidates.Count > 0
                ? Result.Ok<IReadOnlyList<IgdbCandidate>>(candidates)
                : Result.Fail("No matches found. Enter the details by hand instead.");
        }
        catch (HttpRequestException)
        {
            return Result.Fail("Couldn't reach IGDB. Enter the details by hand instead.");
        }
    }

    private async Task<string?> GetTokenAsync(IgdbOptions config, CancellationToken cancellationToken)
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_token is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            {
                return _token;
            }

            var url = $"{TokenUrl}?client_id={Uri.EscapeDataString(config.ClientId)}" +
                      $"&client_secret={Uri.EscapeDataString(config.ClientSecret)}&grant_type=client_credentials";
            using var response = await httpClient.PostAsync(url, null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            _token = document.RootElement.GetProperty("access_token").GetString();
            var lifetime = document.RootElement.GetProperty("expires_in").GetInt32();
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(lifetime - 60);
            return _token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }
}

/// <summary>
///     Represents a candidate match from an IGDB lookup.
/// </summary>
/// <param name="Title">The title of the game.</param>
/// <param name="Slug">The IGDB slug of the game.</param>
/// <param name="Year">The release year, or <see langword="null" /> if unknown.</param>
public sealed record IgdbCandidate(string Title, string Slug, int? Year);
