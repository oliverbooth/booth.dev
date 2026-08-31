using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service responsible for managing creative content (artwork, music, etc.).
/// </summary>
public sealed class CreationService
{
    private const string Area = "content";

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly CdnMediaService _cdnMediaService;

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
    ///     Gets a read-only view of the artwork items, excluding trashed ones.
    /// </summary>
    /// <param name="visibility">
    ///     The visibility of the artwork items to retrieve. A value of <see cref="Visibility.None" /> will retrieve every
    ///     non-trashed item regardless of visibility.
    /// </param>
    /// <returns>A read-only list of <see cref="ArtworkItem" /> objects.</returns>
    public IReadOnlyList<ArtworkItem> GetArtworkItems(Visibility visibility = Visibility.Published)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var items = dbContext.ArtworkItems.Where(a => a.TrashedAt == null);
        return
        [
            .. (visibility == Visibility.None ? items : items.Where(a => a.Visibility == visibility))
                .OrderByDescending(a => a.PublishedAt)
        ];
    }

    /// <summary>
    ///     Gets a read-only view of the music items, excluding trashed ones.
    /// </summary>
    /// <param name="visibility">
    ///     The visibility of the music items to retrieve. A value of <see cref="Visibility.None" /> will retrieve every
    ///     non-trashed item regardless of visibility.
    /// </param>
    /// <returns>A read-only list of <see cref="MusicItem" /> objects.</returns>
    public IReadOnlyList<MusicItem> GetMusicItems(Visibility visibility = Visibility.Published)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var items = dbContext.MusicItems.Where(m => m.TrashedAt == null);
        return
        [
            .. (visibility == Visibility.None ? items : items.Where(m => m.Visibility == visibility))
                .OrderByDescending(m => m.PublishedAt)
        ];
    }

    /// <summary>
    ///     Gets every non-trashed artwork item, regardless of visibility, newest first.
    /// </summary>
    /// <returns>A read-only view of every artwork item.</returns>
    public IReadOnlyList<ArtworkItem> GetAllArtworkItems()
    {
        return GetArtworkItems(Visibility.None);
    }

    /// <summary>
    ///     Gets every non-trashed music item, regardless of visibility, newest first.
    /// </summary>
    /// <returns>A read-only view of every music item.</returns>
    public IReadOnlyList<MusicItem> GetAllMusicItems()
    {
        return GetMusicItems(Visibility.None);
    }

    /// <summary>
    ///     Retrieves an artwork item by its ID.
    /// </summary>
    /// <param name="id">The ID of the artwork item.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the item if it's trashed. Only the admin editor should pass <see langword="true" /> - every
    ///     public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the item if found; otherwise, an error result.</returns>
    public Result<ArtworkItem> GetArtworkItem(Guid id, bool includeTrashed = false)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.ArtworkItems.Find(id);
        if (item is null || (item.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The artwork item with ID {id} was not found");
        }

        return item;
    }

    /// <summary>
    ///     Retrieves a music item by its ID.
    /// </summary>
    /// <param name="id">The ID of the music item.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the item if it's trashed. Only the admin editor should pass <see langword="true" /> - every
    ///     public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the item if found; otherwise, an error result.</returns>
    public Result<MusicItem> GetMusicItem(Guid id, bool includeTrashed = false)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.MusicItems.Find(id);
        if (item is null || (item.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The music item with ID {id} was not found");
        }

        return item;
    }

    /// <summary>
    ///     Gets every trashed artwork item, newest-trashed first.
    /// </summary>
    /// <returns>A read-only view of every trashed artwork item.</returns>
    public IReadOnlyList<ArtworkItem> GetTrashedArtworkItems()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return [.. dbContext.ArtworkItems.Where(a => a.TrashedAt != null).OrderByDescending(a => a.TrashedAt)];
    }

    /// <summary>
    ///     Gets every trashed music item, newest-trashed first.
    /// </summary>
    /// <returns>A read-only view of every trashed music item.</returns>
    public IReadOnlyList<MusicItem> GetTrashedMusicItems()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return [.. dbContext.MusicItems.Where(m => m.TrashedAt != null).OrderByDescending(m => m.TrashedAt)];
    }

    /// <summary>
    ///     Creates a new artwork item.
    /// </summary>
    /// <param name="request">The artwork item's fields.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created item.</returns>
    public Result<ArtworkItem> CreateArtworkItem(ArtworkItemSaveRequest request)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var item = new ArtworkItem();
        ApplyArtworkRequest(item, request);

        dbContext.ArtworkItems.Add(item);
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Updates an existing artwork item.
    /// </summary>
    /// <param name="id">The ID of the artwork item to update.</param>
    /// <param name="request">The item's new fields.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated item, or an error if no item with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<ArtworkItem> UpdateArtworkItem(Guid id, ArtworkItemSaveRequest request)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.ArtworkItems.Find(id);

        if (item is null)
        {
            return Result.Fail($"The artwork item with ID {id} was not found");
        }

        ApplyArtworkRequest(item, request);
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Moves an artwork item to the trash. It's excluded from every listing, but nothing about it is otherwise touched, and
    ///     it can be restored with <see cref="RestoreArtworkItem" />.
    /// </summary>
    /// <param name="id">The ID of the artwork item to trash.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the trashed item, or an error if no item with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<ArtworkItem> TrashArtworkItem(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.ArtworkItems.Find(id);

        if (item is null)
        {
            return Result.Fail($"The artwork item with ID {id} was not found");
        }

        item.TrashedAt = DateTimeOffset.UtcNow;
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Restores a trashed artwork item, making it visible in listings again.
    /// </summary>
    /// <param name="id">The ID of the artwork item to restore.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the restored item, or an error if no item with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<ArtworkItem> RestoreArtworkItem(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.ArtworkItems.Find(id);

        if (item is null)
        {
            return Result.Fail($"The artwork item with ID {id} was not found");
        }

        item.TrashedAt = null;
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Permanently deletes a trashed artwork item - the item row and its uploaded file on the CDN. This cannot be undone.
    /// </summary>
    /// <param name="id">The ID of the artwork item to permanently delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if no item with the specified <paramref name="id" /> exists
    ///     or it isn't currently trashed.
    /// </returns>
    public Result PermanentlyDeleteArtworkItem(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.ArtworkItems.Find(id);

        if (item is null)
        {
            return Result.Fail($"The artwork item with ID {id} was not found");
        }

        if (item.TrashedAt is null)
        {
            return Result.Fail("Only trashed items can be permanently deleted.");
        }

        _cdnMediaService.DeleteAllMedia(id, item.PublishedAt, Area);

        dbContext.ArtworkItems.Remove(item);
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Creates a new music item.
    /// </summary>
    /// <param name="request">The music item's fields.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created item.</returns>
    public Result<MusicItem> CreateMusicItem(MusicItemSaveRequest request)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var item = new MusicItem();
        ApplyMusicRequest(item, request);

        dbContext.MusicItems.Add(item);
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Updates an existing music item.
    /// </summary>
    /// <param name="id">The ID of the music item to update.</param>
    /// <param name="request">The item's new fields.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated item, or an error if no item with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<MusicItem> UpdateMusicItem(Guid id, MusicItemSaveRequest request)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.MusicItems.Find(id);

        if (item is null)
        {
            return Result.Fail($"The music item with ID {id} was not found");
        }

        ApplyMusicRequest(item, request);
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Moves a music item to the trash. It's excluded from every listing, but nothing about it is otherwise
    ///     touched, and it can be restored with <see cref="RestoreMusicItem" />.
    /// </summary>
    /// <param name="id">The ID of the music item to trash.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the trashed item, or an error if no item with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<MusicItem> TrashMusicItem(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.MusicItems.Find(id);

        if (item is null)
        {
            return Result.Fail($"The music item with ID {id} was not found");
        }

        item.TrashedAt = DateTimeOffset.UtcNow;
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Restores a trashed music item, making it visible in listings again.
    /// </summary>
    /// <param name="id">The ID of the music item to restore.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the restored item, or an error if no item with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<MusicItem> RestoreMusicItem(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.MusicItems.Find(id);

        if (item is null)
        {
            return Result.Fail($"The music item with ID {id} was not found");
        }

        item.TrashedAt = null;
        dbContext.SaveChanges();

        return item;
    }

    /// <summary>
    ///     Permanently deletes a trashed music item - the item row and its uploaded file on the CDN. This cannot be undone.
    /// </summary>
    /// <param name="id">The ID of the music item to permanently delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if no item with the specified <paramref name="id" /> exists
    ///     or it isn't currently trashed.
    /// </returns>
    public Result PermanentlyDeleteMusicItem(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var item = dbContext.MusicItems.Find(id);

        if (item is null)
        {
            return Result.Fail($"The music item with ID {id} was not found");
        }

        if (item.TrashedAt is null)
        {
            return Result.Fail("Only trashed items can be permanently deleted.");
        }

        _cdnMediaService.DeleteAllMedia(id, item.PublishedAt, Area);

        dbContext.MusicItems.Remove(item);
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Applies the fields of a save request onto an artwork item.
    /// </summary>
    /// <param name="item">The artwork item to apply the request to.</param>
    /// <param name="request">The save request containing the fields to apply.</param>
    private static void ApplyArtworkRequest(ArtworkItem item, ArtworkItemSaveRequest request)
    {
        item.Title = request.Title;
        item.Description = request.Description;
        item.PublishedAt = request.PublishedAt.ToUniversalTime();
        item.Visibility = request.Visibility;
        item.IsWorkInProgress = request.IsWorkInProgress;
        item.MadeWith = request.MadeWith;
        item.FileName = request.FileName;
        item.Resolution = request.Resolution;
    }

    /// <summary>
    ///     Applies the fields of a save request onto a music item.
    /// </summary>
    /// <param name="item">The music item to apply the request to.</param>
    /// <param name="request">The save request containing the fields to apply.</param>
    private static void ApplyMusicRequest(MusicItem item, MusicItemSaveRequest request)
    {
        item.Title = request.Title;
        item.Description = request.Description;
        item.PublishedAt = request.PublishedAt.ToUniversalTime();
        item.Visibility = request.Visibility;
        item.IsWorkInProgress = request.IsWorkInProgress;
        item.MadeWith = request.MadeWith;
        item.FileName = request.FileName;
        item.Duration = request.Duration;
    }
}
