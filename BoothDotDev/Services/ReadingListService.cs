using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service which fetches books from the reading list.
/// </summary>
public sealed class ReadingListService
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
    /// <returns>A collection of books in the specified state, ordered by author and then sort key.</returns>
    public IReadOnlyCollection<Book> GetBooks(BookState state)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var books = state == (BookState)(-1)
            ? context.Books.ToArray()
            : context.Books.Where(b => b.State == state).ToArray();
        return Sort(books);
    }

    /// <summary>
    ///     Gets every book in the reading list, regardless of state.
    /// </summary>
    /// <returns>A collection of every book in the reading list.</returns>
    public IReadOnlyCollection<Book> GetAllBooks()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return Sort(context.Books.ToArray());
    }

    /// <summary>
    ///     Gets the total number of books in the reading list.
    /// </summary>
    /// <returns>The total number of books in the reading list.</returns>
    public int GetBookCount()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Books.Count();
    }

    /// <summary>
    ///     Adds a new book to the reading list.
    /// </summary>
    /// <param name="book">The book to add.</param>
    /// <returns>A <see cref="Result{T}" /> containing the added book, or an error if its ISBN is already in use.</returns>
    public Result<Book> AddBook(Book book)
    {
        using var context = _dbContextFactory.CreateDbContext();
        if (context.Books.Any(b => b.Isbn == book.Isbn))
        {
            return Result.Fail($"A book with ISBN '{book.Isbn}' is already on the list.");
        }

        context.Books.Add(book);
        context.SaveChanges();
        return Result.Ok(book);
    }

    /// <summary>
    ///     Sets the state of a book on the reading list.
    /// </summary>
    /// <param name="isbn">The ISBN of the book to update.</param>
    /// <param name="state">The new state.</param>
    /// <returns>A <see cref="Result" /> indicating success, or an error if no book with the specified ISBN was found.</returns>
    public Result SetState(string isbn, BookState state)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var book = context.Books.Find(isbn);
        if (book is null)
        {
            return Result.Fail($"No book with ISBN '{isbn}' was found.");
        }

        book.State = state;
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Removes a book from the reading list.
    /// </summary>
    /// <param name="isbn">The ISBN of the book to remove.</param>
    /// <returns>A <see cref="Result" /> indicating success, or an error if no book with the specified ISBN was found.</returns>
    public Result DeleteBook(string isbn)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var book = context.Books.Find(isbn);
        if (book is null)
        {
            return Result.Fail($"No book with ISBN '{isbn}' was found.");
        }

        context.Books.Remove(book);
        context.SaveChanges();
        return Result.Ok();
    }

    private static Book[] Sort(Book[] books)
    {
        return books.OrderBy(b => b.Author).ThenBy(b => b.SortKey).ToArray();
    }
}
