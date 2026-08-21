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
    ///     Creates a new note.
    /// </summary>
    /// <param name="title">The title of the note.</param>
    /// <param name="content">The content of the note.</param>
    /// <param name="fontStyle">The font style of the note.</param>
    /// <param name="visibility">The visibility of the note.</param>
    /// <param name="publishedAt">The publication date and time of the note.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created note.</returns>
    public Result<Note> CreateNote(string title, string content, FontStyle fontStyle, Visibility visibility, DateTimeOffset publishedAt)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var note = new Note
        {
            Title = title,
            Content = content,
            FontStyle = fontStyle,
            Visibility = visibility,
            Published = publishedAt.ToUniversalTime()
        };

        context.Notes.Add(note);
        context.SaveChanges();
        return note;
    }

    /// <summary>
    ///     Updates an existing note.
    /// </summary>
    /// <param name="id">The ID of the note to update.</param>
    /// <param name="title">The title of the note.</param>
    /// <param name="content">The content of the note.</param>
    /// <param name="fontStyle">The font style of the note.</param>
    /// <param name="visibility">The visibility of the note.</param>
    /// <param name="publishedAt">The publication date and time of the note.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated note, or an error if no note with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Note> UpdateNote(Guid id, string title, string content, FontStyle fontStyle, Visibility visibility, DateTimeOffset publishedAt)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var note = context.Notes.Find(id);

        if (note is null)
        {
            return Result.Fail($"The note with ID {id} was not found");
        }

        note.Title = title;
        note.Content = content;
        note.FontStyle = fontStyle;
        note.Visibility = visibility;
        note.Published = publishedAt.ToUniversalTime();
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
        var notes = context.Notes.Where(n => n.TrashedAt == null);
        if (visibility != Visibility.None)
        {
            notes = notes.Where(n => n.Visibility == visibility);
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
        var note = context.Notes.FirstOrDefault(note => note.Id == id);

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
            _ => context.Notes.Count(n => n.TrashedAt == null && n.Visibility == visibility)
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
        var notes = context.Notes.Where(n => n.TrashedAt == null);

        if (searchOptions.Visibility != Visibility.None)
        {
            notes = notes.Where(n => n.Visibility == searchOptions.Visibility);
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
        return [.. context.Notes.Where(n => n.TrashedAt != null).OrderByDescending(n => n.TrashedAt)];
    }
}
