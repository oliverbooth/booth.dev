using BoothDotDev.Common.Data;

namespace BoothDotDev.Common.Services;

/// <summary>
///     Represents a service for searching content.
/// </summary>
public interface ISearchService
{
    /// <summary>
    ///     Searches for content matching the specified search text.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <returns>A collection of search results.</returns>
    Task<IReadOnlyCollection<SearchResult>> SearchAsync(string searchText);
}
