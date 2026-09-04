using BoothDotDev.Data;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Cdn;

/// <summary>
///     Represents the page model for the admin CDN file browser.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class Index : PageModel
{
    private readonly CdnBrowserService _cdnBrowserService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="cdnBrowserService">The <see cref="CdnBrowserService" />.</param>
    public Index(CdnBrowserService cdnBrowserService)
    {
        _cdnBrowserService = cdnBrowserService;
    }

    /// <summary>
    ///     Gets or sets the directory currently being browsed, relative to the CDN root.
    /// </summary>
    /// <value>The current directory, or <see langword="null" /> for the CDN root.</value>
    [BindProperty(SupportsGet = true)]
    public string? Path { get; set; }

    /// <summary>
    ///     Gets the files and folders in the current directory.
    /// </summary>
    /// <value>The current directory's contents.</value>
    public IReadOnlyList<CdnEntry> Entries { get; private set; } = [];

    /// <summary>
    ///     Gets the breadcrumb trail for the current directory, root first.
    /// </summary>
    /// <value>The breadcrumb trail.</value>
    public IReadOnlyList<CdnBreadcrumb> Breadcrumbs { get; private set; } = [];

    /// <summary>
    ///     Gets the normalized, root-relative path of the directory currently being browsed (e.g. <c>/foo/bar</c>, or <c>/</c>
    ///     for the root).
    /// </summary>
    /// <value>The current directory's normalized display path.</value>
    public string CurrentDisplayPath { get; private set; } = "/";

    /// <summary>
    ///     Handles the GET request, listing the current directory's contents.
    /// </summary>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet()
    {
        var result = _cdnBrowserService.ListDirectory(Path);
        if (result.IsFailed)
        {
            // an invalid or nonexistent ?path= (including a traversal attempt) bounces safely to the root
            return RedirectToPage(new { path = (string?)null });
        }

        Entries = result.Value.Entries;
        Breadcrumbs = BuildBreadcrumbs(result.Value.ResolvedPath.Segments);
        CurrentDisplayPath = result.Value.ResolvedPath.DisplayPath;
        return Page();
    }

    /// <summary>
    ///     Handles the POST request for creating a new, empty subfolder in the current directory.
    /// </summary>
    /// <param name="name">The new folder's bare name.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostNewFolder(string name)
    {
        return ToJsonResult(_cdnBrowserService.CreateFolder(Path, name));
    }

    /// <summary>
    ///     Handles the POST request for uploading a new file into the current directory.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public async Task<IActionResult> OnPostUploadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        return ToJsonResult(await _cdnBrowserService.UploadAsync(Path, file, cancellationToken));
    }

    /// <summary>
    ///     Handles the POST request for renaming a file or folder in the current directory.
    /// </summary>
    /// <param name="name">The current bare name.</param>
    /// <param name="newName">The new bare name.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRename(string name, string newName)
    {
        return ToJsonResult(_cdnBrowserService.Rename(Path, name, newName));
    }

    /// <summary>
    ///     Handles the POST request for previewing how many items a recursive delete would remove.
    /// </summary>
    /// <param name="name">The bare name to preview deleting.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDeletePreview(string name)
    {
        var result = _cdnBrowserService.PreviewDelete(Path, name);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(new { itemCount = result.Value.ItemCount, capped = result.Value.Capped });
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting a file, or a folder and everything inside it.
    /// </summary>
    /// <param name="name">The bare name to delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(string name)
    {
        var result = _cdnBrowserService.Delete(Path, name);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(new { ok = true });
    }

    /// <summary>
    ///     Handles the POST request for moving a file or folder into a different directory.
    /// </summary>
    /// <param name="name">The bare name to move.</param>
    /// <param name="destination">The destination directory, relative to the CDN root.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostMove(string name, string destination)
    {
        return ToJsonResult(_cdnBrowserService.Move(Path, name, destination));
    }

    /// <summary>
    ///     Resolves the Tabler icon class for a directory entry, based on its media kind.
    /// </summary>
    /// <param name="entry">The entry to resolve an icon for.</param>
    /// <returns>The Tabler icon class, e.g. <c>ti-folder</c>.</returns>
    public static string GetIcon(CdnEntry entry)
    {
        if (entry.IsDirectory)
        {
            return "ti-folder";
        }

        return entry.Kind switch
        {
            MediaKind.Image => "ti-photo",
            MediaKind.Video => "ti-video",
            MediaKind.Audio => "ti-music",
            _ => "ti-file"
        };
    }

    /// <summary>
    ///     Formats a byte count into a human-readable string with appropriate units (B, KiB, MiB, GiB).
    /// </summary>
    /// <param name="bytes">The size, in bytes.</param>
    /// <returns>The formatted size.</returns>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        string[] units = ["KiB", "MiB", "GiB"];
        var value = bytes / 1024d;
        var unitIndex = 0;

        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return $"{value.ToString(value < 10 ? "0.#" : "0")} {units[unitIndex]}";
    }

    private static IActionResult ToJsonResult<T>(Result<T> result)
    {
        return result.IsFailed
            ? new BadRequestObjectResult(result.Errors.Select(e => e.Message))
            : new JsonResult(new { ok = true });
    }

    private static IReadOnlyList<CdnBreadcrumb> BuildBreadcrumbs(IReadOnlyList<string> segments)
    {
        var breadcrumbs = new List<CdnBreadcrumb> { new("CDN", null) };
        var accumulated = string.Empty;

        foreach (var segment in segments)
        {
            accumulated += "/" + segment;
            breadcrumbs.Add(new CdnBreadcrumb(segment, accumulated));
        }

        return breadcrumbs;
    }
}

/// <summary>
///     Represents a single crumb in the CDN browser's breadcrumb trail.
/// </summary>
/// <param name="Name">The display name of the directory this crumb represents.</param>
/// <param name="Path">The path to navigate to, relative to the CDN root, or <see langword="null" /> for the root.</param>
public sealed record CdnBreadcrumb(string Name, string? Path);
