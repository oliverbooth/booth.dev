using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Creations.Music;

using MusicItem = Data.Models.MusicItem;

/// <summary>
///     Represents the page model for editing a music item in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class Edit : PageModel
{
    private const string Area = "content";

    private readonly CreationService _creationService;
    private readonly CdnMediaService _cdnMediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="creationService">The creation service.</param>
    /// <param name="cdnMediaService">The CDN media service.</param>
    public Edit(CreationService creationService, CdnMediaService cdnMediaService)
    {
        _creationService = creationService;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets or sets the music item being edited, if any.
    /// </summary>
    /// <value>The item being edited, or default values if a new item is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new music item is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new item is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the music item being edited.
    /// </summary>
    /// <value>The ID of the item being edited, or <see langword="null" /> if a new item is being created.</value>
    public Guid? ItemId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the music item being edited is trashed.
    /// </summary>
    /// <value><see langword="true" /> if the item is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Gets the CDN URL of the item's currently-uploaded file, if any.
    /// </summary>
    /// <value>The file's CDN URL, or <see langword="null" /> if no file has been uploaded yet.</value>
    public string? FileUrl { get; private set; }

    /// <summary>
    ///     Gets the duration of the item's currently-uploaded track, formatted for display.
    /// </summary>
    /// <value>The duration, formatted as e.g. "3:41", or <see langword="null" /> if no file has been uploaded yet.</value>
    public string? DurationDisplay { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the item to edit. If <see langword="null" />, a new item will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        if (id is null)
        {
            CreatingNew = true;
            Input = new EditModel
            {
                Visibility = Visibility.Private,
                Published = DateTimeOffset.UtcNow.ToLocalTime()
            };
            return Page();
        }

        var itemResult = _creationService.GetMusicItem(id.Value, includeTrashed: true);
        if (itemResult.IsFailed)
        {
            return NotFound();
        }

        var item = itemResult.Value;
        ItemId = item.Id;
        IsTrashed = item.TrashedAt is not null;
        Input = new EditModel
        {
            Title = item.Title,
            Description = item.Description,
            Published = item.Published.ToLocalTime(),
            Visibility = item.Visibility,
            IsWorkInProgress = item.IsWorkInProgress,
            MadeWith = item.MadeWith
        };
        PopulateFileDisplay(item);

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the item's metadata fields.
    /// </summary>
    /// <param name="id">The ID of the item being edited. If <see langword="null" />, a new item is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    /// <remarks>
    ///     The uploaded file (if any) is untouched. It's managed separately by <see cref="OnPostUploadFileAsync" />.
    /// </remarks>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;
        ItemId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string fileName;
        TimeSpan duration;

        if (id is null)
        {
            fileName = string.Empty;
            duration = TimeSpan.Zero;
        }
        else
        {
            var existingResult = _creationService.GetMusicItem(id.Value, includeTrashed: true);
            if (existingResult.IsFailed)
            {
                return NotFound();
            }

            fileName = existingResult.Value.FileName;
            duration = existingResult.Value.Duration;
        }

        var request = new MusicItemSaveRequest(
            Input.Title,
            Input.Description,
            Input.Published,
            Input.Visibility,
            Input.IsWorkInProgress,
            Input.MadeWith,
            fileName,
            duration);

        var result = id is null
            ? _creationService.CreateMusicItem(request)
            : _creationService.UpdateMusicItem(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for uploading (or replacing) the item's audio file. Every other field is left
    ///     untouched.
    /// </summary>
    /// <param name="id">The ID of the item being edited.</param>
    /// <param name="file">The uploaded audio file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public async Task<IActionResult> OnPostUploadFileAsync(Guid id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        var itemResult = _creationService.GetMusicItem(id, includeTrashed: true);
        if (itemResult.IsFailed)
        {
            return NotFound();
        }

        var item = itemResult.Value;

        if (!string.IsNullOrEmpty(item.FileName))
        {
            _cdnMediaService.DeleteFile(id, item.Published, item.FileName, Area);
        }

        var uploadResult = await _cdnMediaService.UploadAsync(id, item.Published, file, Area, cancellationToken);
        if (uploadResult.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, uploadResult.Errors.Select(e => e.Message)));
            ItemId = id;
            Input = new EditModel
            {
                Title = item.Title,
                Description = item.Description,
                Published = item.Published.ToLocalTime(),
                Visibility = item.Visibility,
                IsWorkInProgress = item.IsWorkInProgress,
                MadeWith = item.MadeWith
            };
            IsTrashed = item.TrashedAt is not null;
            PopulateFileDisplay(item);
            return Page();
        }

        var fileName = uploadResult.Value.FileName;

        // read duration straight from the file that UploadAsync just wrote to disk, at the same path
        // CdnMediaService.GetDirectory (private) would resolve to for this area/kind/date/id
        var kind = CdnMediaResolver.ResolveMediaKind(fileName);
        var filePath = Path.Combine(
            CdnPaths.GetRoot(),
            Area,
            kind.ToString().ToLowerInvariant(),
            item.Published.ToString("yyyy"),
            item.Published.ToString("MM"),
            id.ToString("N"),
            fileName);

        var duration = TagLib.File.Create(filePath).Properties.Duration;

        var request = new MusicItemSaveRequest(
            item.Title,
            item.Description,
            item.Published,
            item.Visibility,
            item.IsWorkInProgress,
            item.MadeWith,
            fileName,
            duration);

        _creationService.UpdateMusicItem(id, request);

        return RedirectToPage(new { id });
    }

    /// <summary>
    ///     Handles the POST request for moving the item to the trash.
    /// </summary>
    /// <param name="id">The ID of the item to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        ItemId = id;
        return RedirectOnSuccess(_creationService.TrashMusicItem(id));
    }

    /// <summary>
    ///     Handles the POST request for restoring the item from the trash.
    /// </summary>
    /// <param name="id">The ID of the item to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        ItemId = id;
        return RedirectOnSuccess(_creationService.RestoreMusicItem(id));
    }

    /// <summary>
    ///     Populates <see cref="FileUrl" /> and <see cref="DurationDisplay" /> from the given item.
    /// </summary>
    private void PopulateFileDisplay(MusicItem item)
    {
        if (string.IsNullOrEmpty(item.FileName))
        {
            return;
        }

        var kind = CdnMediaResolver.ResolveMediaKind(item.FileName);
        FileUrl = CdnMediaResolver.BuildCdnUrl(Area, kind, item.Published, item.Id, item.FileName);
        DurationDisplay = item.Duration.ToString(item.Duration.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
    }

    /// <summary>
    ///     Redirects back to this item's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<MusicItem> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a music item.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the track.
        /// </summary>
        /// <value>The title.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the description of the track.
        /// </summary>
        /// <value>The description, or <see langword="null" /> if the track has no description.</value>
        public string? Description { get; set; }

        /// <summary>
        ///     Gets or sets the publication date and time of the track.
        /// </summary>
        /// <value>The publication date and time.</value>
        public DateTimeOffset Published { get; set; }

        /// <summary>
        ///     Gets or sets the visibility of the track.
        /// </summary>
        /// <value>The visibility.</value>
        public Visibility Visibility { get; set; } = Visibility.Private;

        /// <summary>
        ///     Gets or sets a value indicating whether the track is a work in progress.
        /// </summary>
        /// <value><see langword="true" /> if the track is a work in progress; otherwise, <see langword="false" />.</value>
        public bool IsWorkInProgress { get; set; }

        /// <summary>
        ///     Gets or sets a string describing how the track was made.
        /// </summary>
        /// <value>The "made with" string, or <see langword="null" /> if not specified.</value>
        public string? MadeWith { get; set; }
    }
}
