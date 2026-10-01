using System.ComponentModel.DataAnnotations;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Comments;

/// <summary>
///     Represents the page model for editing a single legacy comment.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly CommentService _commentService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="commentService">The comment service.</param>
    public Edit(CommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    ///     Gets or sets the form input.
    /// </summary>
    /// <value>The form input.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets the original author of the comment, used to link back to the author's page.
    /// </summary>
    /// <value>The author's name.</value>
    public string Author { get; private set; } = string.Empty;

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the comment.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid id)
    {
        if (_commentService.GetLegacyComment(id) is not { } comment)
        {
            return NotFound();
        }

        Author = comment.Author;
        Input = new EditModel {Author = comment.Author, Body = comment.Body};
        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the comment.
    /// </summary>
    /// <param name="id">The ID of the comment.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid id)
    {
        if (!ModelState.IsValid)
        {
            Author = Input.Author;
            return Page();
        }

        if (!_commentService.UpdateLegacyComment(id, Input.Author.Trim(), Input.Body))
        {
            return NotFound();
        }

        return RedirectToPage(new {id});
    }

    /// <summary>
    ///     Handles the POST request for deleting the comment.
    /// </summary>
    /// <param name="id">The ID of the comment.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        if (_commentService.GetLegacyComment(id) is not { } comment)
        {
            return NotFound();
        }

        if (!_commentService.DeleteLegacyComment(id))
        {
            ModelState.AddModelError(string.Empty, "This comment has replies, so it can't be deleted.");
            Author = comment.Author;
            Input = new EditModel {Author = comment.Author, Body = comment.Body};
            return Page();
        }

        return RedirectToPage("Author", new {name = comment.Author});
    }

    /// <summary>
    ///     Represents the editable fields of a legacy comment.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the author's name.
        /// </summary>
        /// <value>The author's name.</value>
        [Required, StringLength(50)]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the comment body.
        /// </summary>
        /// <value>The comment body.</value>
        [Required, StringLength(32767)]
        public string Body { get; set; } = string.Empty;
    }
}
