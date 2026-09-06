using System.Text.Json;
using System.Text.RegularExpressions;
using FluentResults;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for looking up book metadata from Open Library, to help fill in a reading list entry
///     from just a title or an ISBN.
/// </summary>
/// <param name="httpClient">The <see cref="HttpClient" /> to use for making requests to the Open Library API.</param>
public sealed partial class BookLookupService(HttpClient httpClient)
{
    private const int MaxResults = 8;
    private const string SearchUrl = "https://openlibrary.org/search.json";

    /// <summary>
    ///     Searches Open Library for books matching the given query, treating it as an ISBN if it looks like one,
    ///     and as a free-text title search otherwise.
    /// </summary>
    /// <param name="query">The title or ISBN to search for.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the matching candidates, or an error if none were found.</returns>
    public async Task<Result<IReadOnlyList<BookLookupCandidate>>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        query = query.Trim();
        if (query.Length == 0)
        {
            return Result.Fail("Enter a title or ISBN to search for.");
        }

        var isbn = NormalizeIsbn(query);
        var isIsbnQuery = LooksLikeIsbn(isbn);
        var url = isIsbnQuery
            ? $"{SearchUrl}?isbn={Uri.EscapeDataString(isbn)}&fields=title,author_name,isbn&limit={MaxResults}"
            : $"{SearchUrl}?title={Uri.EscapeDataString(query)}&fields=title,author_name,isbn&limit={MaxResults}";

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(url, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result.Fail("Couldn't reach Open Library. Enter the details by hand instead.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail("Couldn't reach Open Library. Enter the details by hand instead.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var candidates = new List<BookLookupCandidate>();
            foreach (var doc in document.RootElement.GetProperty("docs").EnumerateArray())
            {
                if (!doc.TryGetProperty("title", out var titleProperty) || titleProperty.GetString() is not { Length: > 0 } title)
                {
                    continue;
                }

                var author = doc.TryGetProperty("author_name", out var authors) && authors.GetArrayLength() > 0
                    ? authors[0].GetString() ?? string.Empty
                    : string.Empty;

                // An ISBN query already names the exact edition; a title query has to guess one from the work's list
                // of every known edition.
                var candidateIsbn = isIsbnQuery ? isbn : PickIsbn(doc);
                if (candidateIsbn is null)
                {
                    continue;
                }

                candidates.Add(new BookLookupCandidate(title, author, candidateIsbn));
            }

            return candidates.Count > 0
                ? Result.Ok<IReadOnlyList<BookLookupCandidate>>(candidates)
                : Result.Fail("No matches found. Enter the details by hand instead.");
        }
    }

    /// <summary>
    ///     Picks a representative ISBN for a search result, preferring a 13-digit one since that's the modern
    ///     standard.
    /// </summary>
    /// <param name="doc">The search result to pick an ISBN from.</param>
    /// <returns>The picked ISBN, or <see langword="null" /> if the result has none.</returns>
    private static string? PickIsbn(JsonElement doc)
    {
        if (!doc.TryGetProperty("isbn", out var isbnArray) || isbnArray.GetArrayLength() == 0)
        {
            return null;
        }

        var isbns = isbnArray.EnumerateArray().Select(e => e.GetString()).OfType<string>().ToArray();
        return isbns.FirstOrDefault(i => i.Length == 13) ?? isbns.FirstOrDefault();
    }

    /// <summary>
    ///     Strips everything but letters and digits from an ISBN, so hyphens or spaces copied from a book jacket
    ///     don't break lookup or storage.
    /// </summary>
    /// <param name="isbn">The raw ISBN.</param>
    /// <returns>The normalized ISBN, in uppercase.</returns>
    internal static string NormalizeIsbn(string isbn)
    {
        return new string(isbn.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }

    /// <summary>
    ///     Determines whether the given (already-normalized) string is shaped like an ISBN-10 or ISBN-13, without
    ///     validating its check digit.
    /// </summary>
    /// <param name="normalizedIsbn">The normalized candidate ISBN.</param>
    /// <returns><see langword="true" /> if the string is ISBN-shaped; otherwise, <see langword="false" />.</returns>
    private static bool LooksLikeIsbn(string normalizedIsbn)
    {
        return IsbnShapeRegex().IsMatch(normalizedIsbn);
    }

    [GeneratedRegex(@"^\d{9}[\dX]$|^\d{13}$")]
    private static partial Regex IsbnShapeRegex();
}

/// <summary>
///     Represents a candidate match from a book lookup.
/// </summary>
/// <param name="Title">The title of the book.</param>
/// <param name="Author">The author of the book.</param>
/// <param name="Isbn">The ISBN of the book.</param>
public sealed record BookLookupCandidate(string Title, string Author, string Isbn);
