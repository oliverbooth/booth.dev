using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Extensions;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Notes;

using Note = Data.Models.Note;

/// <summary>
///     Represents the page model for editing a note in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly NoteService _noteService;
    private readonly MarkdownRenderingService _markdownRenderingService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="noteService">The note service.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    public Edit(NoteService noteService, MarkdownRenderingService markdownRenderingService)
    {
        _noteService = noteService;
        _markdownRenderingService = markdownRenderingService;
    }

    /// <summary>
    ///     Gets or sets the note being edited, if any.
    /// </summary>
    /// <value>The note being edited, or default values if a new note is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new note is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new note is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the note being edited.
    /// </summary>
    /// <value>The ID of the note being edited, or <see langword="null" /> if a new note is being created.</value>
    public Guid? NoteId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the note being edited is trashed.
    /// </summary>
    /// <value><see langword="true" /> if the note is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the note to edit. If <see langword="null" />, a new note will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            Input = new EditModel
            {
                Visibility = Visibility.Published,
                FontStyle = FontStyle.Serif,
                PublishedAt = DateTimeOffset.UtcNow
            };
            return Page();
        }

        var result = _noteService.GetNoteById(id.Value, includeTrashed: true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var note = result.Value;
        NoteId = note.Id;
        IsTrashed = note.TrashedAt is not null;
        Input = new EditModel
        {
            Title = note.Title,
            Content = note.Content,
            FontStyle = note.FontStyle,
            Visibility = note.Visibility,
            PublishedAt = note.Published
        };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the note.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = id is null
            ? _noteService.CreateNote(Input.Title, Input.Content, Input.FontStyle, Input.Visibility, Input.PublishedAt)
            : _noteService.UpdateNote(id.Value, Input.Title, Input.Content, Input.FontStyle, Input.Visibility, Input.PublishedAt);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for rendering a live preview of the note content.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <returns>
    ///     A JSON payload of the rendered preview HTML and the prose CSS class for the note's font style. This
    ///     handler backs the editor's live-updating preview pane and is only ever called via <c>fetch</c> — there's
    ///     no server-rendered fallback, since the Markdown editor itself already requires JS to function.
    /// </returns>
    public IActionResult OnPostPreview(Guid? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var html = _markdownRenderingService.Render(Input.Content, id ?? Guid.Empty, Input.PublishedAt);
        var proseClass = Input.FontStyle.ToProseClass();

        return new JsonResult(new { html, proseClass });
    }

    /// <summary>
    ///     Handles the POST request for moving the note to the trash.
    /// </summary>
    /// <param name="id">The ID of the note to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid? id)
    {
        if (id is not { } noteId)
        {
            return BadRequest("Save the note before it can be trashed.");
        }

        return RedirectOnSuccess(_noteService.TrashNote(noteId));
    }

    /// <summary>
    ///     Handles the POST request for restoring the note from the trash.
    /// </summary>
    /// <param name="id">The ID of the note to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid? id)
    {
        if (id is not { } noteId)
        {
            return BadRequest("Save the note before it can be restored.");
        }

        return RedirectOnSuccess(_noteService.RestoreNote(noteId));
    }

    /// <summary>
    ///     Redirects back to this note's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<Note> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a note.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the note.
        /// </summary>
        /// <value>The title of the note.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the content of the note.
        /// </summary>
        /// <value>The content of the note.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the font style of the note.
        /// </summary>
        /// <value>The font style of the note.</value>
        public FontStyle FontStyle { get; set; } = FontStyle.Serif;

        /// <summary>
        ///     Gets or sets the visibility of the note.
        /// </summary>
        /// <value>The visibility of the note.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;

        /// <summary>
        ///     Gets or sets the publication date and time of the note.
        /// </summary>
        /// <value>The publication date and time of the note.</value>
        public DateTimeOffset PublishedAt { get; set; }
    }
}
