using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Books;

/// <summary>
///     Represents the page model for the admin books page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly ReadingListService _readingListService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="readingListService">The reading list service.</param>
    public Index(ReadingListService readingListService)
    {
        _readingListService = readingListService;
    }

    /// <summary>
    ///     Gets every book on the reading list.
    /// </summary>
    /// <value>Every book on the reading list.</value>
    public IReadOnlyCollection<Book> Books { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Books = _readingListService.GetAllBooks();
    }

    /// <summary>
    ///     Handles the POST request for changing a book's state.
    /// </summary>
    /// <param name="isbn">The ISBN of the book to update.</param>
    /// <param name="state">The new state.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSetState(string isbn, BookState state)
    {
        _readingListService.SetState(isbn, state);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for removing a book from the reading list.
    /// </summary>
    /// <param name="isbn">The ISBN of the book to remove.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(string isbn)
    {
        _readingListService.DeleteBook(isbn);
        return RedirectToPage();
    }
}
