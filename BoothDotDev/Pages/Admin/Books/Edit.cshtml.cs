using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Books;

/// <summary>
///     Represents the page model for editing a book on the reading list.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly ReadingListService _readingListService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="readingListService">The reading list service.</param>
    public Edit(ReadingListService readingListService)
    {
        _readingListService = readingListService;
    }

    /// <summary>
    ///     Gets the ISBN of the book being edited.
    /// </summary>
    /// <value>The ISBN of the book being edited.</value>
    public string Isbn { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the book being edited.
    /// </summary>
    /// <value>The book being edited.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="isbn">The ISBN of the book to edit.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(string isbn)
    {
        var result = _readingListService.GetBookByIsbn(isbn);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var book = result.Value;
        Isbn = book.Isbn;
        Input = new EditModel { Title = book.Title, Author = book.Author, SortTitle = book.SortTitle, State = book.State };
        return Page();
    }

    /// <summary>
    ///     Handles the POST request for recomputing the sort title from a title.
    /// </summary>
    /// <param name="title">The title to compute the sort title of.</param>
    /// <returns>A JSON payload of the sort title, or <see langword="null" /> if the title needs none.</returns>
    public IActionResult OnPostSortTitle(string? title)
    {
        return new JsonResult(new { sortTitle = SortTitles.Recompute(title ?? string.Empty) });
    }

    /// <summary>
    ///     Handles the POST request for saving the book.
    /// </summary>
    /// <param name="isbn">The ISBN of the book being edited.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(string isbn)
    {
        Isbn = isbn;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = _readingListService.UpdateBook(isbn, Input.Title.Trim(), Input.Author.Trim(), Input.State,
            SortTitles.Normalize(Input.SortTitle));
        if (result.IsFailed)
        {
            return NotFound();
        }

        return RedirectToPage("Index");
    }

    /// <summary>
    ///     Represents the model for editing a book on the reading list.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the book.
        /// </summary>
        /// <value>The title of the book.</value>
        [Required]
        [StringLength(64)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the sort title, which overrides the default of filing under the title minus a leading article.
        /// </summary>
        /// <value>The sort title, or <see langword="null" /> to file it by its title.</value>
        [StringLength(64)]
        public string? SortTitle { get; set; }

        /// <summary>
        ///     Gets or sets the author of the book.
        /// </summary>
        /// <value>The author of the book.</value>
        [Required]
        [StringLength(64)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the state of the book.
        /// </summary>
        /// <value>The state of the book.</value>
        public BookState State { get; set; }
    }
}
