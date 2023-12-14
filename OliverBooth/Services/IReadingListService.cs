using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service which fetches books from the reading list.
/// </summary>
public interface IReadingListService
{
    /// <summary>
    ///     Gets the books in the reading list with the specified state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>A collection of books in the specified state.</returns>
    IReadOnlyCollection<IBook> GetBooks(BookState state);
}
