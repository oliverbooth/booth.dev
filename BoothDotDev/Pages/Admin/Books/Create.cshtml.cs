using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Books;

/// <summary>
///     Represents the page model for adding a book to the reading list.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Create : PageModel
{
    private readonly BookLookupService _bookLookupService;
    private readonly ReadingListService _readingListService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Create" /> class.
    /// </summary>
    /// <param name="readingListService">The reading list service.</param>
    /// <param name="bookLookupService">The book lookup service.</param>
    public Create(ReadingListService readingListService, BookLookupService bookLookupService)
    {
        _readingListService = readingListService;
        _bookLookupService = bookLookupService;
    }

    /// <summary>
    ///     Gets or sets the book being created.
    /// </summary>
    /// <value>The book being created.</value>
    [BindProperty]
    public CreateModel Input { get; set; } = new();

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    ///     Handles the POST request for looking up book metadata by title or ISBN.
    /// </summary>
    /// <param name="query">The title or ISBN to search for.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of matching candidates, or an error message if none were found.</returns>
    public async Task<IActionResult> OnPostLookupAsync(string? query, CancellationToken cancellationToken)
    {
        var result = await _bookLookupService.SearchAsync(query ?? string.Empty, cancellationToken);
        return result.IsFailed
            ? new JsonResult(new { error = result.Errors[0].Message })
            : new JsonResult(new { candidates = result.Value });
    }

    /// <summary>
    ///     Handles the POST request for adding the book to the reading list.
    /// </summary>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var book = new Book
        {
            Isbn = BookLookupService.NormalizeIsbn(Input.Isbn),
            Title = Input.Title.Trim(),
            Author = Input.Author.Trim(),
            State = Input.State
        };

        var result = _readingListService.AddBook(book);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage("Index");
    }

    /// <summary>
    ///     Represents the model for adding a book to the reading list.
    /// </summary>
    public sealed class CreateModel
    {
        /// <summary>
        ///     Gets or sets the ISBN of the book. Set once - there's no editing it afterward, so a typo means
        ///     deleting the entry and adding it again.
        /// </summary>
        /// <value>The ISBN of the book.</value>
        [Required]
        [StringLength(13, MinimumLength = 10)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the title of the book.
        /// </summary>
        /// <value>The title of the book.</value>
        [Required]
        [StringLength(64)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

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
        public BookState State { get; set; } = BookState.PlanToRead;
    }
}
