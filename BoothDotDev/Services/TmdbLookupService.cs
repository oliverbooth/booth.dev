using System.Net.Http.Headers;
using System.Text.Json;
using BoothDotDev.Data;
using FluentResults;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for looking up movie/TV metadata from The Movie Database (TMDB), to help fill in a
///     watchlist entry from just a title.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient" /> to use for making requests to the TMDB API.</param>
/// <param name="options">The TMDB options.</param>
public sealed class TmdbLookupService(HttpClient httpClient, IOptionsMonitor<TmdbOptions> options)
{
    private const int MaxResults = 8;
    private const string SearchUrl = "https://api.themoviedb.org/3/search/multi";

    /// <summary>
    ///     Searches TMDB for movies and TV shows matching the given title.
    /// </summary>
    /// <param name="query">The title to search for.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the matching candidates, or an error if none were found.</returns>
    public async Task<Result<IReadOnlyList<TmdbCandidate>>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        query = query.Trim();
        if (query.Length == 0)
        {
            return Result.Fail("Enter a title to search for.");
        }

        var apiKey = options.CurrentValue.ApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            return Result.Fail("TMDB isn't configured. Enter the details by hand instead.");
        }

        var url = $"{SearchUrl}?query={Uri.EscapeDataString(query)}&include_adult=false";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result.Fail("Couldn't reach TMDB. Enter the details by hand instead.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail("Couldn't reach TMDB. Enter the details by hand instead.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var candidates = new List<TmdbCandidate>();
            foreach (var result in document.RootElement.GetProperty("results").EnumerateArray())
            {
                var mediaType = result.TryGetProperty("media_type", out var mediaTypeProperty)
                    ? mediaTypeProperty.GetString()
                    : null;

                // "multi" search also returns people; those have neither a "title" nor a "name" that makes sense here
                var kind = mediaType switch
                {
                    "movie" => WatchableKind.Movie,
                    "tv" => WatchableKind.Show,
                    _ => (WatchableKind?)null
                };

                if (kind is not { } resolvedKind)
                {
                    continue;
                }

                var titleField = resolvedKind == WatchableKind.Movie ? "title" : "name";
                if (!result.TryGetProperty(titleField, out var titleProperty) ||
                    titleProperty.GetString() is not { Length: > 0 } title)
                {
                    continue;
                }

                var dateField = resolvedKind == WatchableKind.Movie ? "release_date" : "first_air_date";
                var year = result.TryGetProperty(dateField, out var dateProperty) &&
                           dateProperty.GetString() is { Length: >= 4 } date
                    ? int.Parse(date[..4])
                    : (int?)null;

                candidates.Add(new TmdbCandidate(title, resolvedKind, year));

                if (candidates.Count >= MaxResults)
                {
                    break;
                }
            }

            return candidates.Count > 0
                ? Result.Ok<IReadOnlyList<TmdbCandidate>>(candidates)
                : Result.Fail("No matches found. Enter the details by hand instead.");
        }
    }
}

/// <summary>
///     Represents a candidate match from a TMDB lookup.
/// </summary>
/// <param name="Title">The title of the movie or show.</param>
/// <param name="Kind">The kind of the item.</param>
/// <param name="Year">The release year, or <see langword="null" /> if unknown.</param>
public sealed record TmdbCandidate(string Title, WatchableKind Kind, int? Year);
