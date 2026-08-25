using BoothDotDev.Data;
using BoothDotDev.Markdown.Link;
using FluentResults;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for managing the files a post has uploaded to the CDN, mirroring the exact folder layout
///     that <see cref="CdnMediaResolver" /> resolves Markdown media references against.
/// </summary>
public sealed class CdnMediaService
{
    private readonly ILogger<CdnMediaService> _logger;
    private readonly string _root;
    private readonly string _baseUrl;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CdnMediaService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="cdnOptions">The CDN options.</param>
    public CdnMediaService(ILogger<CdnMediaService> logger, IOptions<CdnOptions> cdnOptions)
    {
        _logger = logger;
        _root = CdnPaths.GetRoot();
        _baseUrl = cdnOptions.Value.BaseUrl;
    }

    /// <summary>
    ///     Lists the files currently uploaded for a post, across every media-kind bucket.
    /// </summary>
    /// <param name="id">The post's ID.</param>
    /// <param name="published">The post's published date, forming part of the CDN path.</param>
    /// <param name="area">The content area (e.g. blog, tutorials, projects) used in the CDN path.</param>
    /// <returns>The files uploaded for the post, ordered by filename.</returns>
    public IReadOnlyList<CdnMediaFile> ListFiles(Guid id, DateTimeOffset published, string area)
    {
        var files = new List<CdnMediaFile>();

        foreach (MediaKind kind in Enum.GetValues<MediaKind>())
        {
            var directory = GetDirectory(area, kind, published, id);
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var path in Directory.EnumerateFiles(directory))
            {
                var info = new FileInfo(path);
                files.Add(new CdnMediaFile
                {
                    FileName = info.Name,
                    Url = CdnMediaResolver.BuildCdnUrl(_baseUrl, area, kind, published, id, info.Name),
                    Kind = kind,
                    SizeBytes = info.Length,
                    ModifiedAt = info.LastWriteTimeUtc
                });
            }
        }

        return [.. files.OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    ///     Uploads a new file for a post, stripping EXIF/IPTC/XMP metadata if it's a raster image format that carries it.
    /// </summary>
    /// <param name="id">The post's ID.</param>
    /// <param name="published">The post's published date, forming part of the CDN path.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="area">The content area (e.g. blog, tutorials, projects) used in the CDN path.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A result containing the uploaded file's metadata, or a failure describing why the upload was rejected.</returns>
    public async Task<Result<CdnMediaFile>> UploadAsync(
        Guid id,
        DateTimeOffset published,
        IFormFile file,
        string area,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return Result.Fail("The uploaded file is empty.");
        }

        if (file.Length > CdnUploadPolicy.MaxUploadSizeBytes)
        {
            return Result.Fail($"The uploaded file exceeds the {CdnUploadPolicy.MaxUploadSizeBytes / (1024 * 1024)} MB upload limit.");
        }

        var nameResult = ValidateFileName(file.FileName);
        if (nameResult.IsFailed)
        {
            return nameResult.ToResult<CdnMediaFile>();
        }

        var fileName = nameResult.Value;
        var extension = Path.GetExtension(fileName).TrimStart('.');
        if (!CdnUploadPolicy.AllowedExtensions.Contains(extension))
        {
            return Result.Fail($"Files with the extension '.{extension}' aren't allowed.");
        }

        var kind = CdnMediaResolver.ResolveMediaKind(fileName);
        var directory = GetDirectory(area, kind, published, id);
        var destinationPath = Path.Combine(directory, fileName);

        if (File.Exists(destinationPath))
        {
            return Result.Fail($"A file named '{fileName}' already exists for this post. Rename or delete it first.");
        }

        Directory.CreateDirectory(directory);
        var tempPath = $"{destinationPath}.{Path.GetRandomFileName()}.tmp";

        try
        {
            await using (var source = file.OpenReadStream())
            await using (var destination = File.Create(tempPath))
            {
                if (CdnUploadPolicy.StrippableImageExtensions.Contains(extension))
                {
                    using var raw = new MemoryStream();
                    await source.CopyToAsync(raw, cancellationToken);
                    raw.Position = 0;
                    CdnUploadPolicy.StripImageMetadata(raw, destination);
                }
                else
                {
                    await source.CopyToAsync(destination, cancellationToken);
                }
            }

            File.Move(tempPath, destinationPath);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Rejected upload {FileName} for post {PostId}: could not process file", fileName, id);
            TryDelete(tempPath);
            return Result.Fail(
                "The uploaded file couldn't be processed. If it's an image, it may be corrupt or an unsupported variant.");
        }

        var info = new FileInfo(destinationPath);
        return new CdnMediaFile
        {
            FileName = fileName,
            Url = CdnMediaResolver.BuildCdnUrl(_baseUrl, area, kind, published, id, fileName),
            Kind = kind,
            SizeBytes = info.Length,
            ModifiedAt = info.LastWriteTimeUtc
        };
    }

    /// <summary>
    ///     Deletes a previously-uploaded file from a post.
    /// </summary>
    /// <param name="id">The post's ID.</param>
    /// <param name="published">The post's published date, forming part of the CDN path.</param>
    /// <param name="fileName">The bare filename to delete.</param>
    /// <param name="area">The content area (e.g. blog, tutorials, projects) used in the CDN path.</param>
    /// <returns>A result indicating success, or a failure describing why the file couldn't be deleted.</returns>
    public Result DeleteFile(Guid id, DateTimeOffset published, string fileName, string area)
    {
        var nameResult = ValidateFileName(fileName);
        if (nameResult.IsFailed)
        {
            return nameResult.ToResult();
        }

        var kind = CdnMediaResolver.ResolveMediaKind(nameResult.Value);
        var path = Path.Combine(GetDirectory(area, kind, published, id), nameResult.Value);

        if (!File.Exists(path))
        {
            return Result.Fail($"'{fileName}' was not found.");
        }

        File.Delete(path);
        return Result.Ok();
    }

    /// <summary>
    ///     Renames a previously-uploaded file.
    /// </summary>
    /// <param name="id">The post's ID.</param>
    /// <param name="published">The post's published date, forming part of the CDN path.</param>
    /// <param name="fileName">The current bare filename.</param>
    /// <param name="newFileName">The new bare filename.</param>
    /// <param name="area">The content area (e.g. blog, tutorials, projects) used in the CDN path.</param>
    /// <returns>A result containing the renamed file's metadata, or a failure describing why the rename was rejected.</returns>
    /// <remarks>
    ///     The file extension can't be changed by a rename, since that would move it into a different media-kind bucket.
    /// </remarks>
    public Result<CdnMediaFile> RenameFile(Guid id, DateTimeOffset published, string fileName, string newFileName, string area)
    {
        var oldNameResult = ValidateFileName(fileName);
        if (oldNameResult.IsFailed)
        {
            return oldNameResult.ToResult<CdnMediaFile>();
        }

        var newNameResult = ValidateFileName(newFileName);
        if (newNameResult.IsFailed)
        {
            return newNameResult.ToResult<CdnMediaFile>();
        }

        var oldName = oldNameResult.Value;
        var newName = newNameResult.Value;

        if (!string.Equals(Path.GetExtension(oldName), Path.GetExtension(newName), StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail("Renaming can't change a file's extension.");
        }

        var kind = CdnMediaResolver.ResolveMediaKind(oldName);
        var directory = GetDirectory(area, kind, published, id);
        var oldPath = Path.Combine(directory, oldName);
        var newPath = Path.Combine(directory, newName);

        if (!File.Exists(oldPath))
        {
            return Result.Fail($"'{fileName}' was not found.");
        }

        if (!string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase) && File.Exists(newPath))
        {
            return Result.Fail($"A file named '{newFileName}' already exists.");
        }

        File.Move(oldPath, newPath, overwrite: false);

        var info = new FileInfo(newPath);
        return new CdnMediaFile
        {
            FileName = newName,
            Url = CdnMediaResolver.BuildCdnUrl(_baseUrl, area, kind, published, id, newName),
            Kind = kind,
            SizeBytes = info.Length,
            ModifiedAt = info.LastWriteTimeUtc
        };
    }

    /// <summary>
    ///     Moves a post's entire media folder from one published date to another.
    /// </summary>
    /// <param name="id">The post's ID.</param>
    /// <param name="oldPublished">The post's previous published date.</param>
    /// <param name="newPublished">The post's new published date.</param>
    /// <param name="area">The content area (e.g. blog, tutorials, projects) used in the CDN path.</param>
    /// <returns>A result indicating success, or a failure describing why the move couldn't be completed.</returns>
    /// <remarks>
    ///     The CDN URL a Markdown reference resolves to is derived from the post's <em>current</em> published date, so if that
    ///     date changes after files have already been uploaded, the physical files have to move to match or every reference to
    ///     them 404s.
    /// </remarks>
    public Result MoveDate(Guid id, DateTimeOffset oldPublished, DateTimeOffset newPublished, string area)
    {
        if (oldPublished.Year == newPublished.Year && oldPublished.Month == newPublished.Month)
        {
            return Result.Ok();
        }

        foreach (MediaKind kind in Enum.GetValues<MediaKind>())
        {
            var oldDirectory = GetDirectory(area, kind, oldPublished, id);
            if (!Directory.Exists(oldDirectory))
            {
                continue;
            }

            var newDirectory = GetDirectory(area, kind, newPublished, id);
            if (Directory.Exists(newDirectory))
            {
                return Result.Fail(
                    $"Can't move media for post '{id:N}': a folder already exists at the destination date.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(newDirectory)!);
            Directory.Move(oldDirectory, newDirectory);

            PruneEmptyParents(
                Path.GetDirectoryName(oldDirectory)!,
                stopAt: Path.Combine(_root, area, kind.ToString().ToLowerInvariant()));
        }

        return Result.Ok();
    }

    /// <summary>
    ///     Permanently deletes every file uploaded for a post, across every media-kind bucket.
    /// </summary>
    /// <param name="id">The post's ID.</param>
    /// <param name="published">The post's published date, forming part of the CDN path.</param>
    /// <param name="area">The content area (e.g. blog, tutorials, projects) used in the CDN path.</param>
    /// <remarks>
    ///     Each post's media lives in its own ID-keyed directory, never shared with any other post, so this is safe to
    ///     call unconditionally when a post is permanently deleted - there's nothing else that could be referencing
    ///     the files being removed.
    /// </remarks>
    public void DeleteAllMedia(Guid id, DateTimeOffset published, string area)
    {
        foreach (MediaKind kind in Enum.GetValues<MediaKind>())
        {
            var directory = GetDirectory(area, kind, published, id);
            if (!Directory.Exists(directory))
            {
                continue;
            }

            Directory.Delete(directory, recursive: true);

            PruneEmptyParents(
                Path.GetDirectoryName(directory)!,
                stopAt: Path.Combine(_root, area, kind.ToString().ToLowerInvariant()));
        }
    }

    private static string GetDirectory(string area, MediaKind kind, DateTimeOffset published, Guid id)
    {
        return CdnPaths.GetMediaDirectory(area, kind, published, id);
    }

    private static void PruneEmptyParents(string directory, string stopAt)
    {
        var current = directory;
        while (!string.Equals(current, stopAt, StringComparison.OrdinalIgnoreCase) &&
               current.StartsWith(stopAt, StringComparison.OrdinalIgnoreCase) &&
               Directory.Exists(current) &&
               !Directory.EnumerateFileSystemEntries(current).Any())
        {
            Directory.Delete(current);
            current = Path.GetDirectoryName(current)!;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
    }

    private static Result<string> ValidateFileName(string? fileName)
    {
        return CdnNaming.ValidateSegment(Path.GetFileName(fileName ?? string.Empty));
    }
}
