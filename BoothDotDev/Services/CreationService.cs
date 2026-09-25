using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;
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
    ///     Retrieves a non-trashed creation by its slug.
    /// </summary>
    /// <param name="slug">The slug of the creation.</param>
    /// <returns>A <see cref="Result{T}" /> containing the creation if found; otherwise, an error result.</returns>
    public Result<Creation> GetCreationBySlug(string slug)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.Creations.FirstOrDefault(c => c.Slug == slug && c.TrashedAt == null);
        return item is null ? Result.Fail($"The creation with slug '{slug}' was not found") : item;
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

        var slugResult = ResolveSlug(dbContext, request, null);
        if (slugResult.IsFailed)
        {
            return slugResult.ToResult<Creation>();
        }

        var item = new Creation();
        ApplyRequest(item, request, slugResult.Value);

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

        var slugResult = ResolveSlug(dbContext, request, id);
        if (slugResult.IsFailed)
        {
            return slugResult.ToResult<Creation>();
        }

        ApplyRequest(item, request, slugResult.Value);
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

    private static Result<string> ResolveSlug(AppDbContext dbContext, CreationSaveRequest request, Guid? id)
    {
        var slug = (string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug).ToSlug();
        if (slug.Length == 0)
        {
            return Result.Fail("A slug is required, and the title has no letters or digits to make one from.");
        }

        var taken = dbContext.Creations.Any(c => c.Slug == slug && c.Id != id) || dbContext.Projects.Any(p => p.Slug == slug);
        return taken ? Result.Fail($"The slug '{slug}' is already used by another project or creation.") : slug;
    }

    private static void ApplyRequest(Creation item, CreationSaveRequest request, string slug)
    {
        item.Kind = request.Kind;
        item.Title = request.Title;
        item.Slug = slug;
        item.Description = request.Description;
        item.PublishedAt = request.PublishedAt.ToUniversalTime();
        item.Visibility = request.Visibility;
        item.IsWorkInProgress = request.IsWorkInProgress;
        item.Tools = request.Tools;
    }
}
