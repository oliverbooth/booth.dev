using Microsoft.EntityFrameworkCore;
using OliverBooth.Common.Data.Web;
using OliverBooth.Common.Services;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

internal sealed class ReadingListService : IReadingListService
{
    private readonly IDbContextFactory<WebContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadingListService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public ReadingListService(IDbContextFactory<WebContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets the books in the reading list with the specified state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>A collection of books in the specified state.</returns>
    public IReadOnlyCollection<IBook> GetBooks(BookState state)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        return state == (BookState)(-1)
            ? context.Books.ToArray()
            : context.Books.Where(b => b.State == state).ToArray();
    }
}
