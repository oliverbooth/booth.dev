using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Notes;

using Note = Data.Models.Note;

/// <summary>
///     Represents the page model for the admin note trash page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Trash : PageModel
{
    private readonly NoteService _noteService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="noteService">The <see cref="NoteService" />.</param>
    public Trash(NoteService noteService)
    {
        _noteService = noteService;
    }

    /// <summary>
    ///     Gets the list of trashed notes, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed notes.</value>
    public IReadOnlyList<Note> Notes { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Notes = _noteService.GetTrashedNotes();
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed note.
    /// </summary>
    /// <param name="id">The ID of the note to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        _noteService.RestoreNote(id);
        return RedirectToPage();
    }
}
