using BoothDotDev.Data;
using BoothDotDev.Markdown.Link;
using FluentResults;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for browsing and managing arbitrary files and folders under the CDN mount, independent of any single
///     post. Every operation except <see cref="Move" /> resolves against a bare, single-segment <c>name</c> - only the
///     currently-browsed directory and a move's destination ever accept a multi-segment path.
/// </summary>
public sealed class CdnBrowserService
{
    private const string BaseUrl = "https://cdn.booth.dev";

    private readonly ILogger<CdnBrowserService> _logger;
    private readonly string _root;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CdnBrowserService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public CdnBrowserService(ILogger<CdnBrowserService> logger) : this(logger, CdnPaths.GetRoot())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CdnBrowserService" /> class rooted at an explicit directory, bypassing
    ///     the real CDN mount. Exists for tests to point the service at a disposable temp directory.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="root">The root directory to confine every operation to.</param>
    internal CdnBrowserService(ILogger<CdnBrowserService> logger, string root)
    {
        _logger = logger;
        _root = root;
    }

    /// <summary>
    ///     Lists the immediate contents of a directory under the CDN mount.
    /// </summary>
    /// <param name="path">The directory to list, relative to the CDN root.</param>
    /// <returns>A result containing the directory's contents, or a failure describing why it couldn't be listed.</returns>
    public Result<CdnDirectoryListing> ListDirectory(string? path)
    {
        var resolved = ResolveExistingDirectory(path);
        if (resolved.IsFailed)
        {
            return resolved.ToResult<CdnDirectoryListing>();
        }

        var directory = resolved.Value.AbsolutePath;
        var entries = Directory.EnumerateFileSystemEntries(directory)
            .Select(entryPath => Directory.Exists(entryPath)
                ? BuildDirectoryEntry(entryPath, resolved.Value)
                : BuildFileEntry(entryPath, resolved.Value))
            .OrderByDescending(e => e.IsDirectory)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new CdnDirectoryListing(entries, resolved.Value);
    }

    /// <summary>
    ///     Creates a new, empty subfolder.
    /// </summary>
    /// <param name="path">The parent directory, relative to the CDN root.</param>
    /// <param name="name">The new folder's bare name.</param>
    /// <returns>A result containing the created folder's metadata, or a failure describing why it couldn't be created.</returns>
    public Result<CdnEntry> CreateFolder(string? path, string name)
    {
        var dirResult = ResolveExistingDirectory(path);
        if (dirResult.IsFailed)
        {
            return dirResult.ToResult<CdnEntry>();
        }

        var nameResult = CdnNaming.ValidateSegment(name);
        if (nameResult.IsFailed)
        {
            return nameResult.ToResult<CdnEntry>();
        }

        var targetPath = Path.Combine(dirResult.Value.AbsolutePath, nameResult.Value);
        if (File.Exists(targetPath) || Directory.Exists(targetPath))
        {
            return Result.Fail($"'{name}' already exists.");
        }

        Directory.CreateDirectory(targetPath);
        return BuildDirectoryEntry(targetPath, dirResult.Value);
    }

    /// <summary>
    ///     Uploads a new file into a directory, stripping EXIF/IPTC/XMP metadata if it's a raster image format that carries it.
    /// </summary>
    /// <param name="path">The destination directory, relative to the CDN root.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A result containing the uploaded file's metadata, or a failure describing why the upload was rejected.</returns>
    public async Task<Result<CdnEntry>> UploadAsync(string? path, IFormFile file, CancellationToken cancellationToken)
    {
        var dirResult = ResolveExistingDirectory(path);
        if (dirResult.IsFailed)
        {
            return dirResult.ToResult<CdnEntry>();
        }

        if (file.Length == 0)
        {
            return Result.Fail("The uploaded file is empty.");
        }

        if (file.Length > CdnUploadPolicy.MaxUploadSizeBytes)
        {
            return Result.Fail(
                $"The uploaded file exceeds the {CdnUploadPolicy.MaxUploadSizeBytes / (1024 * 1024)} MB upload limit.");
        }

        var nameResult = CdnNaming.ValidateSegment(Path.GetFileName(file.FileName));
        if (nameResult.IsFailed)
        {
            return nameResult.ToResult<CdnEntry>();
        }

        var fileName = nameResult.Value;
        var extension = Path.GetExtension(fileName).TrimStart('.');
        if (!CdnUploadPolicy.AllowedExtensions.Contains(extension))
        {
            return Result.Fail($"Files with the extension '.{extension}' aren't allowed.");
        }

        var destinationPath = Path.Combine(dirResult.Value.AbsolutePath, fileName);
        if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
        {
            return Result.Fail($"'{fileName}' already exists in this folder.");
        }

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
            _logger.LogWarning(ex, "Rejected CDN upload {FileName}: could not process file", fileName);
            TryDelete(tempPath);
            return Result.Fail(
                "The uploaded file couldn't be processed. If it's an image, it may be corrupt or an unsupported variant.");
        }

        return BuildFileEntry(destinationPath, dirResult.Value);
    }

    /// <summary>
    ///     Renames a file or folder in place.
    /// </summary>
    /// <param name="path">The containing directory, relative to the CDN root.</param>
    /// <param name="name">The current bare name.</param>
    /// <param name="newName">The new bare name.</param>
    /// <returns>A result containing the renamed entry's metadata, or a failure describing why the rename was rejected.</returns>
    public Result<CdnEntry> Rename(string? path, string name, string newName)
    {
        var dirResult = ResolveExistingDirectory(path);
        if (dirResult.IsFailed)
        {
            return dirResult.ToResult<CdnEntry>();
        }

        var oldNameResult = CdnNaming.ValidateSegment(name);
        if (oldNameResult.IsFailed)
        {
            return oldNameResult.ToResult<CdnEntry>();
        }

        var newNameResult = CdnNaming.ValidateSegment(newName);
        if (newNameResult.IsFailed)
        {
            return newNameResult.ToResult<CdnEntry>();
        }

        var directory = dirResult.Value.AbsolutePath;
        var oldPath = Path.Combine(directory, oldNameResult.Value);
        var newPath = Path.Combine(directory, newNameResult.Value);

        var isDirectory = Directory.Exists(oldPath);
        if (!isDirectory && !File.Exists(oldPath))
        {
            return Result.Fail($"'{name}' was not found.");
        }

        if (!string.Equals(oldNameResult.Value, newNameResult.Value, StringComparison.OrdinalIgnoreCase) &&
            (File.Exists(newPath) || Directory.Exists(newPath)))
        {
            return Result.Fail($"'{newName}' already exists.");
        }

        if (isDirectory)
        {
            Directory.Move(oldPath, newPath);
        }
        else
        {
            File.Move(oldPath, newPath, overwrite: false);
        }

        return isDirectory ? BuildDirectoryEntry(newPath, dirResult.Value) : BuildFileEntry(newPath, dirResult.Value);
    }

    /// <summary>
    ///     Reports a bounded count of how many items a recursive delete of a folder would remove, without actually deleting
    ///     anything.
    /// </summary>
    /// <param name="path">The containing directory, relative to the CDN root.</param>
    /// <param name="name">The bare name to preview deleting.</param>
    /// <param name="itemCap">The maximum number of items to enumerate before giving up and reporting the count as capped.</param>
    /// <returns>A result containing the preview, or a failure describing why it couldn't be computed.</returns>
    /// <remarks>
    ///     Enumeration is lazy and stops as soon as <paramref name="itemCap" /> is exceeded, so this is bounded to roughly
    ///     <paramref name="itemCap" /> filesystem operations regardless of how large the real folder is.
    /// </remarks>
    public Result<CdnDeletePreview> PreviewDelete(string? path, string name, int itemCap = 500)
    {
        var dirResult = ResolveExistingDirectory(path);
        if (dirResult.IsFailed)
        {
            return dirResult.ToResult<CdnDeletePreview>();
        }

        var nameResult = CdnNaming.ValidateSegment(name);
        if (nameResult.IsFailed)
        {
            return nameResult.ToResult<CdnDeletePreview>();
        }

        var targetPath = Path.Combine(dirResult.Value.AbsolutePath, nameResult.Value);

        if (Directory.Exists(targetPath))
        {
            var count = Directory.EnumerateFileSystemEntries(targetPath, "*", SearchOption.AllDirectories)
                .Take(itemCap + 1)
                .Count();
            return new CdnDeletePreview(Math.Min(count, itemCap), count > itemCap);
        }

        if (File.Exists(targetPath))
        {
            return new CdnDeletePreview(0, false);
        }

        return Result.Fail($"'{name}' was not found.");
    }

    /// <summary>
    ///     Permanently deletes a file, or a folder and everything inside it.
    /// </summary>
    /// <param name="path">The containing directory, relative to the CDN root.</param>
    /// <param name="name">The bare name to delete.</param>
    /// <returns>A result indicating success, or a failure describing why the deletion couldn't be completed.</returns>
    public Result Delete(string? path, string name)
    {
        var dirResult = ResolveExistingDirectory(path);
        if (dirResult.IsFailed)
        {
            return dirResult.ToResult();
        }

        var nameResult = CdnNaming.ValidateSegment(name);
        if (nameResult.IsFailed)
        {
            return nameResult.ToResult();
        }

        var targetPath = Path.Combine(dirResult.Value.AbsolutePath, nameResult.Value);

        if (Directory.Exists(targetPath))
        {
            Directory.Delete(targetPath, recursive: true);
            return Result.Ok();
        }

        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
            return Result.Ok();
        }

        return Result.Fail($"'{name}' was not found.");
    }

    /// <summary>
    ///     Moves a file or folder into a different directory, keeping its name.
    /// </summary>
    /// <param name="path">The current containing directory, relative to the CDN root.</param>
    /// <param name="name">The bare name to move.</param>
    /// <param name="destination">The destination directory, relative to the CDN root.</param>
    /// <returns>A result containing the moved entry's metadata, or a failure describing why the move was rejected.</returns>
    public Result<CdnEntry> Move(string? path, string name, string destination)
    {
        var dirResult = ResolveExistingDirectory(path);
        if (dirResult.IsFailed)
        {
            return dirResult.ToResult<CdnEntry>();
        }

        var nameResult = CdnNaming.ValidateSegment(name);
        if (nameResult.IsFailed)
        {
            return nameResult.ToResult<CdnEntry>();
        }

        var sourcePath = Path.Combine(dirResult.Value.AbsolutePath, nameResult.Value);
        var isDirectory = Directory.Exists(sourcePath);
        if (!isDirectory && !File.Exists(sourcePath))
        {
            return Result.Fail($"'{name}' was not found.");
        }

        var destResult = ResolveExistingDirectory(destination);
        if (destResult.IsFailed)
        {
            return Result.Fail("The destination folder doesn't exist.");
        }

        if (isDirectory)
        {
            var sourceWithSep = sourcePath + Path.DirectorySeparatorChar;
            var destWithSep = destResult.Value.AbsolutePath + Path.DirectorySeparatorChar;
            if (destResult.Value.AbsolutePath == sourcePath ||
                destWithSep.StartsWith(sourceWithSep, StringComparison.Ordinal))
            {
                return Result.Fail("A folder can't be moved into itself or one of its own subfolders.");
            }
        }

        var destinationPath = Path.Combine(destResult.Value.AbsolutePath, nameResult.Value);
        if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
        {
            return Result.Fail($"'{name}' already exists in the destination folder.");
        }

        if (isDirectory)
        {
            Directory.Move(sourcePath, destinationPath);
        }
        else
        {
            File.Move(sourcePath, destinationPath);
        }

        return isDirectory
            ? BuildDirectoryEntry(destinationPath, destResult.Value)
            : BuildFileEntry(destinationPath, destResult.Value);
    }

    private Result<CdnPath> ResolveExistingDirectory(string? path)
    {
        var resolved = CdnPaths.ResolveRelative(_root, path);
        if (resolved.IsFailed)
        {
            return resolved;
        }

        return Directory.Exists(resolved.Value.AbsolutePath)
            ? resolved
            : Result.Fail("The requested folder was not found.");
    }

    private static CdnEntry BuildFileEntry(string fullPath, CdnPath directory)
    {
        var info = new FileInfo(fullPath);
        return new CdnEntry
        {
            Name = info.Name,
            IsDirectory = false,
            Url = BuildUrl(directory, info.Name),
            Kind = CdnMediaResolver.ResolveMediaKind(info.Name),
            SizeBytes = info.Length,
            ModifiedAt = info.LastWriteTimeUtc
        };
    }

    private static CdnEntry BuildDirectoryEntry(string fullPath, CdnPath directory)
    {
        var info = new DirectoryInfo(fullPath);
        return new CdnEntry
        {
            Name = info.Name,
            IsDirectory = true,
            ItemCount = Directory.EnumerateFileSystemEntries(fullPath).Count(),
            ModifiedAt = info.LastWriteTimeUtc
        };
    }

    private static string BuildUrl(CdnPath directory, string name)
    {
        var directoryPath = directory.DisplayPath == "/" ? string.Empty : directory.DisplayPath;
        return $"{BaseUrl}{directoryPath}/{name}";
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
}
