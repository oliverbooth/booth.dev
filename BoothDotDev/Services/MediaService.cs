using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown.Link;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using TagLib;
using File = System.IO.File;
using Image = SixLabors.ImageSharp.Image;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service responsible for the files (images, videos, and audio) that belong to projects and creations.
/// </summary>
public sealed class MediaService
{
    private readonly string _cdnBaseUrl;
    private readonly CdnMediaService _cdnMediaService;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    /// <param name="cdnMediaService">The <see cref="CdnMediaService" />.</param>
    /// <param name="cdnOptions">The CDN options.</param>
    public MediaService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        CdnMediaService cdnMediaService,
        IOptions<CdnOptions> cdnOptions)
    {
        _dbContextFactory = dbContextFactory;
        _cdnMediaService = cdnMediaService;
        _cdnBaseUrl = cdnOptions.Value.BaseUrl;
    }

    /// <summary>
    ///     Finds the project or creation with the specified ID.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <returns>The <see cref="MediaOwner" />, or <see langword="null" /> if there is no project or creation with that ID.</returns>
    public MediaOwner? FindOwner(Guid ownerId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var createdAt = dbContext.Projects.Where(p => p.Id == ownerId).Select(p => (DateTimeOffset?)p.CreatedAt).FirstOrDefault();
        if (createdAt is { } projectDate)
        {
            return new MediaOwner(ownerId, projectDate, MediaOwner.ProjectArea);
        }

        var publishedAt = dbContext.Creations.Where(c => c.Id == ownerId).Select(c => (DateTimeOffset?)c.PublishedAt)
            .FirstOrDefault();
        return publishedAt is { } creationDate ? new MediaOwner(ownerId, creationDate, MediaOwner.CreationArea) : null;
    }

    /// <summary>
    ///     Gets the files of a project or creation, in order.
    /// </summary>
    /// <param name="owner">The owner of the files.</param>
    /// <returns>The files.</returns>
    public IReadOnlyList<MediaItem> GetMedia(MediaOwner owner)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return
        [
            .. dbContext.Media
                .Where(m => m.ProjectId == owner.Id || m.CreationId == owner.Id)
                .OrderBy(m => m.Position)
                .AsEnumerable()
                .Select(m => ToItem(m, owner))
        ];
    }

    /// <summary>
    ///     Gets the files of many projects and creations at once, in order.
    /// </summary>
    /// <param name="owners">The owners of the files.</param>
    /// <returns>The files of each owner, keyed by the owner's ID. An owner with no files has no entry.</returns>
    public IReadOnlyDictionary<Guid, IReadOnlyList<MediaItem>> GetMedia(IReadOnlyCollection<MediaOwner> owners)
    {
        var ownersById = owners.ToDictionary(o => o.Id);
        var ids = ownersById.Keys.ToList();

        using var dbContext = _dbContextFactory.CreateDbContext();
        return dbContext.Media
            .Where(m => (m.ProjectId != null && ids.Contains(m.ProjectId.Value)) ||
                        (m.CreationId != null && ids.Contains(m.CreationId.Value)))
            .OrderBy(m => m.Position)
            .AsEnumerable()
            .GroupBy(m => m.ProjectId ?? m.CreationId!.Value)
            .ToDictionary(g => g.Key, IReadOnlyList<MediaItem> (g) => [.. g.Select(m => ToItem(m, ownersById[g.Key]))]);
    }

    /// <summary>
    ///     Gets the cover of a project or creation.
    /// </summary>
    /// <param name="owner">The owner of the cover.</param>
    /// <returns>The cover, or <see langword="null" /> if the owner has none.</returns>
    public MediaItem? GetCover(MediaOwner owner)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var cover = dbContext.Media.FirstOrDefault(m => (m.ProjectId == owner.Id || m.CreationId == owner.Id) && m.IsCover);
        return cover is null ? null : ToItem(cover, owner);
    }

    /// <summary>
    ///     Uploads a file to a project or creation, after its other files.
    /// </summary>
    /// <param name="owner">The owner of the file.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A result containing the added file, or a failure describing why it was rejected.</returns>
    /// <remarks>The file does not become the cover. Only an explicit choice does that.</remarks>
    public async Task<Result<MediaItem>> AddAsync(MediaOwner owner, IFormFile file, CancellationToken cancellationToken)
    {
        var kind = CdnMediaResolver.ResolveMediaKind(file.FileName);
        if (kind is not (MediaKind.Image or MediaKind.Video or MediaKind.Audio))
        {
            return Result.Fail($"'{file.FileName}' isn't an image, video, or audio file.");
        }

        var uploadResult = await _cdnMediaService.UploadAsync(owner.Id, owner.Date, file, owner.Area, cancellationToken);
        if (uploadResult.IsFailed)
        {
            return uploadResult.ToResult<MediaItem>();
        }

        var fileName = uploadResult.Value.FileName;
        var media = new Media { FileName = fileName };
        if (owner.Area == MediaOwner.ProjectArea)
        {
            media.ProjectId = owner.Id;
        }
        else
        {
            media.CreationId = owner.Id;
        }

        await ReadMetadataAsync(media, owner, kind, cancellationToken);

        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            media.Position = dbContext.Media
                .Where(m => m.ProjectId == owner.Id || m.CreationId == owner.Id)
                .Select(m => (int?)m.Position)
                .Max() + 1 ?? 0;

            dbContext.Media.Add(media);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _cdnMediaService.DeleteFile(owner.Id, owner.Date, fileName, owner.Area);
            throw;
        }

        return ToItem(media, owner);
    }

    /// <summary>
    ///     Removes a file, deleting it from the CDN too.
    /// </summary>
    /// <param name="owner">The owner of the file.</param>
    /// <param name="mediaId">The ID of the file.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the file could not be removed.</returns>
    public Result Remove(MediaOwner owner, Guid mediaId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var media = FindOwned(dbContext, owner.Id, mediaId);
        if (media is null)
        {
            return Result.Fail($"The file with ID {mediaId} was not found");
        }

        _cdnMediaService.DeleteFile(owner.Id, owner.Date, media.FileName, owner.Area);

        dbContext.Media.Remove(media);
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Makes a file its owner's cover, replacing the previous one.
    /// </summary>
    /// <param name="ownerId">The ID of the owner.</param>
    /// <param name="mediaId">The ID of the file.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the file could not be made the cover.</returns>
    public Result SetCover(Guid ownerId, Guid mediaId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var media = FindOwned(dbContext, ownerId, mediaId);
        if (media is null)
        {
            return Result.Fail($"The file with ID {mediaId} was not found");
        }

        if (CdnMediaResolver.ResolveMediaKind(media.FileName) != MediaKind.Image)
        {
            return Result.Fail("Only an image can be the cover.");
        }

        // the old cover has to be cleared before the new one is set, or the one-cover-per-owner index rejects the second update
        using var transaction = dbContext.Database.BeginTransaction();
        dbContext.Media
            .Where(m => (m.ProjectId == ownerId || m.CreationId == ownerId) && m.IsCover)
            .ExecuteUpdate(s => s.SetProperty(m => m.IsCover, false));

        media.IsCover = true;
        dbContext.SaveChanges();
        transaction.Commit();

        return Result.Ok();
    }

    /// <summary>
    ///     Clears a project or creation's cover, so it shows the placeholder.
    /// </summary>
    /// <param name="ownerId">The ID of the owner.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result ClearCover(Guid ownerId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        dbContext.Media
            .Where(m => (m.ProjectId == ownerId || m.CreationId == ownerId) && m.IsCover)
            .ExecuteUpdate(s => s.SetProperty(m => m.IsCover, false));

        return Result.Ok();
    }

    /// <summary>
    ///     Reassigns the position of an owner's files to match their place in <paramref name="orderedIds" />.
    /// </summary>
    /// <param name="ownerId">The ID of the owner.</param>
    /// <param name="orderedIds">The IDs of the files, in their new order.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result Reorder(Guid ownerId, IReadOnlyList<Guid> orderedIds)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var files = dbContext.Media
            .Where(m => (m.ProjectId == ownerId || m.CreationId == ownerId) && orderedIds.Contains(m.Id))
            .ToDictionary(m => m.Id);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            if (files.TryGetValue(orderedIds[i], out var media))
            {
                media.Position = i;
            }
        }

        dbContext.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Sets the alternative text of a file.
    /// </summary>
    /// <param name="ownerId">The ID of the owner.</param>
    /// <param name="mediaId">The ID of the file.</param>
    /// <param name="alt">The alternative text, or <see langword="null" /> or whitespace to clear it.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the text could not be set.</returns>
    public Result SetAlt(Guid ownerId, Guid mediaId, string? alt)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var media = FindOwned(dbContext, ownerId, mediaId);
        if (media is null)
        {
            return Result.Fail($"The file with ID {mediaId} was not found");
        }

        media.Alt = string.IsNullOrWhiteSpace(alt) ? null : alt.Trim();
        dbContext.SaveChanges();

        return Result.Ok();
    }

    private static Media? FindOwned(AppDbContext dbContext, Guid ownerId, Guid mediaId)
    {
        return dbContext.Media.FirstOrDefault(m => m.Id == mediaId && (m.ProjectId == ownerId || m.CreationId == ownerId));
    }

    /// <summary>
    ///     Reads the dimensions of an image or the length of audio from the file just uploaded. A file that can't be read
    ///     is kept, just without those details.
    /// </summary>
    private static async Task ReadMetadataAsync(Media media, MediaOwner owner, MediaKind kind,
        CancellationToken cancellationToken)
    {
        var path = CdnPaths.GetMediaPath(owner.Area, kind, owner.Date, owner.Id, media.FileName);

        try
        {
            switch (kind)
            {
                case MediaKind.Image:
                    await using (var stream = File.OpenRead(path))
                    {
                        var info = await Image.IdentifyAsync(stream, cancellationToken);
                        media.Width = info.Width;
                        media.Height = info.Height;
                    }

                    break;

                case MediaKind.Audio:
                    media.Duration = TagLib.File.Create(path).Properties.Duration;
                    break;
            }
        }
        catch (Exception exception) when (exception is ImageFormatException or NotSupportedException or CorruptFileException
                                              or UnsupportedFormatException)
        {
            // an SVG, say, isn't something ImageSharp can measure
        }
    }

    private MediaItem ToItem(Media media, MediaOwner owner)
    {
        var kind = CdnMediaResolver.ResolveMediaKind(media.FileName);
        var url = CdnMediaResolver.BuildCdnUrl(_cdnBaseUrl, owner.Area, kind, owner.Date, owner.Id, media.FileName);

        return new MediaItem(media.Id, media.FileName, url, kind, media.Alt, media.IsCover, media.Width, media.Height,
            media.Duration);
    }
}
