using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using DotNext;
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
    ///     Gets all notes.
    /// </summary>
    /// <param name="visibility">The visibility of the notes to retrieve.</param>
    /// <returns>A read-only view of all notes.</returns>
    public IReadOnlyList<Note> GetAllNotes(Visibility visibility = Visibility.Published)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return [.. dbContext.Notes.Where(n => n.Visibility == visibility).OrderByDescending(n => n.Published)];
    }

    /// <summary>
    ///     Gets a note by its ID.
    /// </summary>
    /// <param name="id">The ID of the note.</param>
    /// <returns>A <see cref="Result{T}" /> containing the note if found; otherwise, an error result.</returns>
    public Result<Note> GetNoteById(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var note = dbContext.Notes.FirstOrDefault(note => note.Id == id);
        return note ?? new Result<Note>(new Exception($"The note with ID {id} was not found"));
    }

    /// <summary>
    ///     Gets the most recent notes.
    /// </summary>
    /// <param name="count">The number of notes to retrieve.</param>
    /// <param name="visibility">The visibility of the notes to retrieve.</param>
    /// <returns>A read-only view of the most recent notes.</returns>
    public IReadOnlyList<Note> GetRecentNotes(int count, Visibility visibility = Visibility.Published)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return [.. dbContext.Notes.Where(n => n.Visibility == visibility).OrderByDescending(n => n.Published).Take(count)];
    }
}
