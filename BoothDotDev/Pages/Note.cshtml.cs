using BoothDotDev.Data;
using BoothDotDev.Services;
using DEDrake;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;

namespace BoothDotDev.Pages;

/// <summary>
///     Represents the model for the note page.
/// </summary>
public sealed class Note : PageModel
{
    private readonly NoteService _noteService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Note" /> class.
    /// </summary>
    /// <param name="noteService">The <see cref="NoteService" />.</param>
    public Note(NoteService noteService)
    {
        _noteService = noteService;
    }

    /// <summary>
    ///     Gets the retrieved note, if any.
    /// </summary>
    /// <value>
    ///     An <see cref="Optional{T}" /> containing the retrieved note, or <see cref="Optional{T}.None" /> if no note was found.
    /// </value>
    public Option<Data.Models.Note> RetrievedNote { get; private set; } = Option.None<Data.Models.Note>();

    /// <summary>
    ///     Handles the GET request for the note page with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the note to retrieve.</param>
    public IActionResult OnGet([FromRoute(Name = "id")] string id)
    {
        var guid = ShortGuid.Parse(id);
        var result = _noteService.GetNoteById(guid);

        if (result.IsFailed)
        {
            return NotFound();
        }

        var note = result.Value;
        if (note.Visibility == Visibility.Private && User.Identity?.IsAuthenticated != true)
        {
            return NotFound();
        }

        RetrievedNote = Option.Some(note);
        return Page();
    }
}
