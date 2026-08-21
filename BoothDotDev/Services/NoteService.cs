using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for managing notes.
/// </summary>
public sealed class NoteService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NoteService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDbContextFactory{TContext}" />.</param>
    public NoteService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Creates a new note, along with its first draft, which immediately becomes the note's current draft.
    /// </summary>
    /// <param name="request">The note's parent-level fields and the content of its first draft.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created note.</returns>
    public Result<Note> CreateNote(NoteSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var note = new Note
        {
            Published = request.PublishedAt.ToUniversalTime()
        };

        var draft = NewDraft(note.Id, request.Content);
        note.CurrentDraft = draft;

        context.Notes.Add(note);
        context.NoteDrafts.Add(draft);
        context.SaveChanges();

        return note;
    }

    /// <summary>
    ///     Saves a new draft of an existing note, without publishing it.
    /// </summary>
    /// <param name="id">The ID of the note to save a draft for.</param>
    /// <param name="request">The note's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the note the draft was saved for, or an error if no note with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Note> SaveDraft(Guid id, NoteSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var note = context.Notes.Find(id);

        if (note is null)
        {
            return Result.Fail($"Note with ID '{id}' not found.");
        }

        var draft = NewDraft(note.Id, request.Content);
        context.NoteDrafts.Add(draft);

        note.Published = request.PublishedAt.ToUniversalTime();

        context.SaveChanges();
        return note;
    }

    /// <summary>
    ///     Saves a new draft of an existing note and publishes it, making it the note's current draft.
    /// </summary>
    /// <param name="id">The ID of the note to publish.</param>
    /// <param name="request">The note's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated note, or an error if no note with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Note> PublishNote(Guid id, NoteSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var note = context.Notes.Find(id);

        if (note is null)
        {
            return Result.Fail($"Note with ID '{id}' not found.");
        }

        var draft = NewDraft(note.Id, request.Content);
        context.NoteDrafts.Add(draft);

        note.Published = request.PublishedAt.ToUniversalTime();
        note.CurrentDraftId = draft.Id;
        note.Updated = DateTimeOffset.UtcNow;

        context.SaveChanges();
        return note;
    }

    /// <summary>
    ///     Moves a note to the trash. It's excluded from every listing and 404s on its public URL, but nothing
    ///     about it is otherwise touched, and it can be restored with <see cref="RestoreNote" />.
    /// </summary>
    /// <param name="id">The ID of the note to trash.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the trashed note, or an error if no note with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Note> TrashNote(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var note = context.Notes.Find(id);

        if (note is null)
        {
            return Result.Fail($"The note with ID {id} was not found");
        }

        note.TrashedAt = DateTimeOffset.UtcNow;
        context.SaveChanges();
        return note;
    }

    /// <summary>
    ///     Restores a trashed note, making it visible in listings and on its public URL again.
    /// </summary>
    /// <param name="id">The ID of the note to restore.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the restored note, or an error if no note with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Note> RestoreNote(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var note = context.Notes.Find(id);

        if (note is null)
        {
            return Result.Fail($"The note with ID {id} was not found");
        }

        note.TrashedAt = null;
        context.SaveChanges();
        return note;
    }

    /// <summary>
    ///     Gets all notes.
    /// </summary>
    /// <param name="visibility">The visibility of the notes to retrieve.</param>
    /// <returns>A read-only view of all notes, excluding trashed ones.</returns>
    public IReadOnlyList<Note> GetAllNotes(Visibility visibility = Visibility.Published)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var notes = context.Notes.Include(n => n.CurrentDraft).Where(n => n.TrashedAt == null);
        if (visibility != Visibility.None)
        {
            notes = notes.Where(n => n.CurrentDraft!.Visibility == visibility);
        }

        return [.. notes.OrderByDescending(n => n.Published)];
    }

    /// <summary>
    ///     Gets a note by its ID.
    /// </summary>
    /// <param name="id">The ID of the note.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the note if it's trashed. Only the admin editor should pass <see langword="true" /> —
    ///     every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the note if found; otherwise, an error result.</returns>
    public Result<Note> GetNoteById(Guid id, bool includeTrashed = false)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var note = context.Notes.Include(n => n.CurrentDraft).FirstOrDefault(note => note.Id == id);

        if (note is null || (note.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The note with ID {id} was not found");
        }

        return note;
    }

    /// <summary>
    ///     Gets the count of notes based on their visibility.
    /// </summary>
    /// <param name="visibility">
    ///     The visibility of the notes to count. If set to <see cref="Visibility.None" />, counts all notes regardless of
    ///     visibility.
    /// </param>
    /// <returns>The count of notes based on their visibility, excluding trashed ones.</returns>
    public int GetNoteCount(Visibility visibility = Visibility.None)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return visibility switch
        {
            Visibility.None => context.Notes.Count(n => n.TrashedAt == null),
            _ => context.Notes.Count(n => n.TrashedAt == null && n.CurrentDraft!.Visibility == visibility)
        };
    }

    /// <summary>
    ///     Gets the most recent notes.
    /// </summary>
    /// <param name="searchOptions">The options for searching and retrieving notes.</param>
    /// <returns>A read-only view of the most recent notes, excluding trashed ones.</returns>
    public IReadOnlyList<Note> GetRecentNotes(ActivitySearchOptions searchOptions)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var notes = context.Notes.Include(n => n.CurrentDraft).Where(n => n.TrashedAt == null);

        if (searchOptions.Visibility != Visibility.None)
        {
            notes = notes.Where(n => n.CurrentDraft!.Visibility == searchOptions.Visibility);
        }

        var ordered = searchOptions.SortStrategy switch
        {
            ActivitySortStrategy.Published => notes.OrderByDescending(n => n.Published),
            ActivitySortStrategy.Updated => notes.OrderByDescending(n => n.Updated ?? n.Published),
            _ => throw new ArgumentOutOfRangeException(nameof(searchOptions), searchOptions.SortStrategy, "Unknown sort strategy")
        };

        return [.. ordered.Take(searchOptions.Count)];
    }

    /// <summary>
    ///     Gets all trashed notes, newest-trashed first.
    /// </summary>
    /// <returns>A read-only view of all trashed notes.</returns>
    public IReadOnlyList<Note> GetTrashedNotes()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.Notes.Include(n => n.CurrentDraft).Where(n => n.TrashedAt != null).OrderByDescending(n => n.TrashedAt)
        ];
    }

    /// <summary>
    ///     Returns a note's full draft history, newest first.
    /// </summary>
    /// <param name="id">The ID of the note whose draft history to return.</param>
    /// <returns>The note's drafts, newest first.</returns>
    public IReadOnlyList<NoteDraft> GetDraftHistory(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.NoteDrafts.Where(d => d.NoteId == id).OrderByDescending(d => d.CreatedAt)];
    }

    /// <summary>
    ///     Returns a specific draft of the specified note, for viewing without publishing it.
    /// </summary>
    /// <param name="id">The ID of the note the draft belongs to.</param>
    /// <param name="draftId">The ID of the draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the requested draft, or an error if it doesn't exist or doesn't belong to the
    ///     specified note.
    /// </returns>
    public Result<NoteDraft> GetDraft(Guid id, Guid draftId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.NoteDrafts.Find(draftId);

        if (draft is null || draft.NoteId != id)
        {
            return Result.Fail($"Draft '{draftId}' not found for note '{id}'.");
        }

        return draft;
    }

    /// <summary>
    ///     Returns the newest draft of the specified note, which may or may not be the note's current (published) draft.
    /// </summary>
    /// <param name="id">The ID of the note whose newest draft to return.</param>
    /// <returns>A <see cref="Result{T}" /> containing the note's newest draft, or an error if the note has no drafts.</returns>
    public Result<NoteDraft> GetNewestDraft(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.NoteDrafts.Where(d => d.NoteId == id).OrderByDescending(d => d.CreatedAt).FirstOrDefault();

        if (draft is null)
        {
            return Result.Fail($"Note '{id}' has no drafts.");
        }

        return draft;
    }

    /// <summary>
    ///     Builds a new, unsaved draft snapshot for the specified note.
    /// </summary>
    private static NoteDraft NewDraft(Guid noteId, NoteDraftContent content)
    {
        return new NoteDraft
        {
            NoteId = noteId,
            Title = content.Title,
            Content = content.Content,
            FontStyle = content.FontStyle,
            Visibility = content.Visibility
        };
    }
}
