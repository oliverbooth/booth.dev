using BoothDotDev.Services;
using DEDrake;
using DotNext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
    public Optional<Data.Models.Note> RetrievedNote { get; private set; } = Optional<Data.Models.Note>.None;

    /// <summary>
    ///     Handles the GET request for the note page with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the note to retrieve.</param>
    public IActionResult OnGet([FromRoute(Name = "id")] string id)
    {
        var guid = ShortGuid.Parse(id);
        Console.WriteLine($"GUID IS {guid}, as guid: {(Guid)guid}");
        var result = _noteService.GetNoteById(guid);

        if (!result.IsSuccessful)
        {
            return NotFound();
        }

        RetrievedNote = result.Value;
        return Page();
    }
}
