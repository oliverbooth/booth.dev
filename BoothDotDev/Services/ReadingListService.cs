using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which fetches books from the reading list.
/// </summary>
internal sealed class ReadingListService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadingListService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public ReadingListService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets the books in the reading list with the specified state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>A collection of books in the specified state.</returns>
    public IReadOnlyCollection<Book> GetBooks(BookState state)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return state == (BookState)(-1)
            ? context.Books.ToArray()
            : context.Books.Where(b => b.State == state).ToArray();
    }
}
