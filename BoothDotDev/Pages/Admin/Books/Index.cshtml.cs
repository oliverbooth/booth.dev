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
    /// <summary>
    ///     The display order, label, and dot colour for each state's group.
    /// </summary>
    private static readonly (BookState State, string Label, string DotClass)[] StateOrder =
    [
        (BookState.Reading, "reading", "dot"),
        (BookState.PlanToRead, "plan to read", "dot dot-tangerine"),
        (BookState.Read, "read", "dot dot-bubblegum")
    ];

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
    ///     Gets the books, grouped by reading state, in <see cref="StateOrder" />.
    /// </summary>
    /// <value>The state groups.</value>
    public IReadOnlyList<StateGroup> StateGroups { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        var books = _readingListService.GetAllBooks();
        StateGroups = StateOrder
            .Select(s => new StateGroup(s.State, s.Label, s.DotClass, books.Where(b => b.State == s.State).ToArray()))
            .ToArray();
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

    /// <summary>
    ///     Represents a group of books sharing a reading state, for display in the admin listing.
    /// </summary>
    /// <param name="State">The state shared by every book in the group.</param>
    /// <param name="Label">The lowercase display label for the state.</param>
    /// <param name="DotClass">The CSS class for this state's indicator dot.</param>
    /// <param name="Books">The books in this state.</param>
    public sealed record StateGroup(BookState State, string Label, string DotClass, IReadOnlyList<Book> Books);
}
