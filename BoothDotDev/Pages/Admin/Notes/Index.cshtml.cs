using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Notes;

using Note = Data.Models.Note;

/// <summary>
///     Represents the page model for the admin notes page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly NoteService _noteService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="noteService">The <see cref="NoteService" />.</param>
    public Index(NoteService noteService)
    {
        _noteService = noteService;
    }

    /// <summary>
    ///     Gets the list of notes.
    /// </summary>
    /// <value>The list of notes.</value>
    public IReadOnlyList<Note> Notes { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Notes = _noteService.GetAllNotes(Visibility.None);
    }

    /// <summary>
    ///     Handles the POST request for moving a note to the trash.
    /// </summary>
    /// <param name="id">The ID of the note to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        _noteService.TrashNote(id);
        return RedirectToPage();
    }
}
