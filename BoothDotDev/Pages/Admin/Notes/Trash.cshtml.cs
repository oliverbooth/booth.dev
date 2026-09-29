using BoothDotDev.Services;

namespace BoothDotDev.Pages.Admin.Notes;

using Note = Data.Models.Note;

/// <summary>
///     Represents the page model for the admin note trash page.
/// </summary>
public sealed class Trash : TrashPageModel<Note>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="noteService">The <see cref="NoteService" />.</param>
    public Trash(NoteService noteService)
        : base(noteService.GetTrashedNotes, id => noteService.RestoreNote(id), id => noteService.PermanentlyDeleteNote(id))
    {
    }

    /// <summary>
    ///     Gets the list of trashed notes, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed notes.</value>
    public IReadOnlyList<Note> Notes => Items;
}
