using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service responsible for managing creations (drawings, 3D renders, and music).
/// </summary>
public sealed class CreationService
{
    private const string Area = "content";
    private readonly CdnMediaService _cdnMediaService;

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreationService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    /// <param name="cdnMediaService">The <see cref="CdnMediaService" />.</param>
    public CreationService(IDbContextFactory<AppDbContext> dbContextFactory, CdnMediaService cdnMediaService)
    {
        _dbContextFactory = dbContextFactory;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets a read-only view of the creations, excluding trashed ones.
    /// </summary>
    /// <param name="visibility">
    ///     The visibility of the creations to retrieve. A value of <see cref="Visibility.None" /> will retrieve every
    ///     non-trashed creation regardless of visibility.
    /// </param>
    /// <returns>A read-only list of <see cref="Creation" /> objects, newest first.</returns>
    public IReadOnlyList<Creation> GetCreations(Visibility visibility = Visibility.Published)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var items = dbContext.Creations.Where(c => c.TrashedAt == null);
        return
        [
            .. (visibility == Visibility.None ? items : items.Where(c => c.Visibility == visibility))
            .OrderByDescending(c => c.PublishedAt)
        ];
    }

    /// <summary>
    ///     Gets every non-trashed creation, regardless of visibility, newest first.
    /// </summary>
    /// <returns>A read-only view of every creation.</returns>
    public IReadOnlyList<Creation> GetAllCreations()
    {
        return GetCreations(Visibility.None);
    }

    /// <summary>
    ///     Retrieves a creation by its ID.
    /// </summary>
    /// <param name="id">The ID of the creation.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the creation if it's trashed. Only the admin editor should pass <see langword="true" /> -
    ///     every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the creation if found; otherwise, an error result.</returns>
    public Result<Creation> GetCreation(Guid id, bool includeTrashed = false)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.Creations.Find(id);
        if (item is null || (item.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The creation with ID {id} was not found");
        }

        return item;
    }

    /// <summary>
    ///     Gets every trashed creation, most recently trashed first.
    /// </summary>
    /// <returns>A read-only list of trashed <see cref="Creation" /> objects.</returns>
    public IReadOnlyList<Creation> GetTrashedCreations()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return [.. dbContext.Creations.Where(c => c.TrashedAt != null).OrderByDescending(c => c.TrashedAt)];
    }

    /// <summary>
    ///     Creates a new creation.
    /// </summary>
    /// <param name="request">The details of the creation to create.</param>
    /// <returns>A <see cref="Result{T}" /> containing the created creation.</returns>
    public Result<Creation> CreateCreation(CreationSaveRequest request)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var item = new Creation();
        ApplyRequest(item, request);

        dbContext.Creations.Add(item);
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Updates an existing creation.
    /// </summary>
    /// <param name="id">The ID of the creation to update.</param>
    /// <param name="request">The new details of the creation.</param>
    /// <returns>A <see cref="Result{T}" /> containing the updated creation if found; otherwise, an error result.</returns>
    public Result<Creation> UpdateCreation(Guid id, CreationSaveRequest request)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.Creations.Find(id);

        if (item is null)
        {
            return Result.Fail($"The creation with ID {id} was not found");
        }

        ApplyRequest(item, request);
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Moves a creation to the trash.
    /// </summary>
    /// <param name="id">The ID of the creation to trash.</param>
    /// <returns>A <see cref="Result{T}" /> containing the trashed creation if found; otherwise, an error result.</returns>
    public Result<Creation> TrashCreation(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.Creations.Find(id);

        if (item is null)
        {
            return Result.Fail($"The creation with ID {id} was not found");
        }

        item.TrashedAt = DateTimeOffset.UtcNow;
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Restores a creation from the trash.
    /// </summary>
    /// <param name="id">The ID of the creation to restore.</param>
    /// <returns>A <see cref="Result{T}" /> containing the restored creation if found; otherwise, an error result.</returns>
    public Result<Creation> RestoreCreation(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.Creations.Find(id);

        if (item is null)
        {
            return Result.Fail($"The creation with ID {id} was not found");
        }

        item.TrashedAt = null;
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Permanently deletes a trashed creation, along with its media.
    /// </summary>
    /// <param name="id">The ID of the creation to delete.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the creation could not be deleted.</returns>
    public Result PermanentlyDeleteCreation(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.Creations.Find(id);

        if (item is null)
        {
            return Result.Fail($"The creation with ID {id} was not found");
        }

        if (item.TrashedAt is null)
        {
            return Result.Fail("Only trashed items can be permanently deleted.");
        }

        _cdnMediaService.DeleteAllMedia(id, item.PublishedAt, Area);

        dbContext.Creations.Remove(item);
        dbContext.SaveChanges();

        return Result.Ok();
    }

    private static void ApplyRequest(Creation item, CreationSaveRequest request)
    {
        item.Kind = request.Kind;
        item.Title = request.Title;
        item.Description = request.Description;
        item.PublishedAt = request.PublishedAt.ToUniversalTime();
        item.Visibility = request.Visibility;
        item.IsWorkInProgress = request.IsWorkInProgress;
        item.MadeWith = request.MadeWith;
    }
}
