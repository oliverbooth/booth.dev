using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Someday;

using SomedayEntry = SomedayEntry;

/// <summary>
///     Represents the page model for editing a someday entry in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class Edit : PageModel
{
    private const string Area = "someday";
    private readonly CdnMediaService _cdnMediaService;
    private readonly MarkdownRenderingService _markdownRenderingService;

    private readonly SomedayEntryService _somedayEntryService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="somedayEntryService">The someday entry service.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    /// <param name="cdnMediaService">The CDN media service.</param>
    public Edit(SomedayEntryService somedayEntryService, MarkdownRenderingService markdownRenderingService,
        CdnMediaService cdnMediaService)
    {
        _somedayEntryService = somedayEntryService;
        _markdownRenderingService = markdownRenderingService;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets or sets the entry being edited, if any.
    /// </summary>
    /// <value>The entry being edited, or default values if a new entry is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new entry is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new entry is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live (published) for this entry.
    /// </summary>
    /// <value>The ID of the currently-live draft, or <see langword="null" /> if a new entry is being created.</value>
    public Guid? CurrentDraftId { get; private set; }

    /// <summary>
    ///     Gets the entry's full draft history, newest first, for the revision history panel.
    /// </summary>
    /// <value>The entry's drafts, ordered newest first.</value>
    public IReadOnlyList<SomedayEntryDraft> DraftHistory { get; private set; } = [];

    /// <summary>
    ///     Gets the ID of the entry being edited.
    /// </summary>
    /// <value>The ID of the entry being edited, or <see langword="null" /> if a new entry is being created.</value>
    public Guid? EntryId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the entry being edited is trashed.
    /// </summary>
    /// <value><see langword="true" /> if the entry is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft currently loaded into the editor.
    /// </summary>
    /// <value>The ID of the draft being viewed, or <see langword="null" /> if a new entry is being created.</value>
    public Guid? ViewingDraftId { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the entry to edit. If <see langword="null" />, a new entry will be created.</param>
    /// <param name="draftId">
    ///     The ID of a specific draft to view. If <see langword="null" />, the entry's newest draft is loaded - not
    ///     necessarily the currently-live one, so reopening the editor resumes from wherever editing was last left
    ///     off rather than silently discarding unpublished draft work.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id, Guid? draftId)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            Input = new EditModel { Visibility = Visibility.Published };
            return Page();
        }

        var entryResult = _somedayEntryService.GetEntryById(id.Value, true);
        if (entryResult.IsFailed)
        {
            return NotFound();
        }

        var draftResult = draftId.HasValue
            ? _somedayEntryService.GetDraft(id.Value, draftId.Value)
            : _somedayEntryService.GetNewestDraft(id.Value);

        if (draftResult.IsFailed)
        {
            return NotFound();
        }

        var entry = entryResult.Value;
        var draft = draftResult.Value;
        EntryId = entry.Id;
        CurrentDraftId = entry.CurrentDraftId;
        DraftHistory = _somedayEntryService.GetDraftHistory(id.Value);
        IsTrashed = entry.TrashedAt is not null;
        ViewingDraftId = draft.Id;
        Input = new EditModel { Title = draft.Title, Body = draft.Body, Slug = entry.Slug, Visibility = draft.Visibility };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving and publishing the entry, making it the entry's current draft.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = BuildSaveRequest(id);
        var result = id is null
            ? _somedayEntryService.CreateEntry(request)
            : _somedayEntryService.PublishEntry(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for saving a draft of the entry, without publishing it. The entry's
    ///     currently-live draft, if any, is left unchanged.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSaveDraft(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = BuildSaveRequest(id);

        // A brand-new entry has no prior draft to leave untouched, so its first save - draft or not - always
        // becomes the entry's current draft. There's nothing else for it to sensibly point at.
        var result = id is null
            ? _somedayEntryService.CreateEntry(request)
            : _somedayEntryService.SaveDraft(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for rendering a live preview of the entry content.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>
    ///     A JSON payload of the rendered preview HTML. This handler backs the editor's live-updating preview pane
    ///     and is only ever called via <c>fetch</c> - there's no server-rendered fallback, since the Markdown editor
    ///     itself already requires JS to function.
    /// </returns>
    public IActionResult OnPostPreview(Guid? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var html = _markdownRenderingService.Render(Input.Body, id ?? Guid.Empty, DateTimeOffset.UtcNow, Area);
        return new JsonResult(new { html, proseClass = "prose--serif" });
    }

    /// <summary>
    ///     Handles the POST request for moving the entry to the trash.
    /// </summary>
    /// <param name="id">The ID of the entry to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid? id)
    {
        if (id is not { } entryId)
        {
            return BadRequest("Save the entry before it can be trashed.");
        }

        return RedirectOnSuccess(_somedayEntryService.TrashEntry(entryId));
    }

    /// <summary>
    ///     Handles the POST request for restoring the entry from the trash.
    /// </summary>
    /// <param name="id">The ID of the entry to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid? id)
    {
        if (id is not { } entryId)
        {
            return BadRequest("Save the entry before it can be restored.");
        }

        return RedirectOnSuccess(_somedayEntryService.RestoreEntry(entryId));
    }

    /// <summary>
    ///     Handles the POST request for listing the files currently attached to the entry via the CDN.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>A JSON payload of the entry's attached media files.</returns>
    public IActionResult OnPostListMedia(Guid? id)
    {
        if (id is not { } entryId)
        {
            return BadRequest("Save the entry before managing media.");
        }

        return new JsonResult(MediaListPayload(entryId));
    }

    /// <summary>
    ///     Handles the POST request for uploading a new file to the entry's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of the entry's attached media files, including the newly-uploaded one.</returns>
    public async Task<IActionResult> OnPostUploadMediaAsync(Guid? id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (id is not { } entryId)
        {
            return BadRequest("Save the entry before managing media.");
        }

        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        var result = await _cdnMediaService.UploadAsync(entryId, DateTimeOffset.UtcNow, file, Area, cancellationToken);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(entryId));
    }

    /// <summary>
    ///     Handles the POST request for deleting a file from the entry's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <param name="fileName">The bare filename to delete.</param>
    /// <returns>A JSON payload of the entry's remaining attached media files.</returns>
    public IActionResult OnPostDeleteMedia(Guid? id, string fileName)
    {
        if (id is not { } entryId)
        {
            return BadRequest("Save the entry before managing media.");
        }

        var result = _cdnMediaService.DeleteFile(entryId, DateTimeOffset.UtcNow, fileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(entryId));
    }

    /// <summary>
    ///     Handles the POST request for renaming a file in the entry's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <param name="fileName">The current bare filename.</param>
    /// <param name="newFileName">The new bare filename. Its extension must match the current one.</param>
    /// <returns>A JSON payload of the entry's attached media files, reflecting the rename.</returns>
    public IActionResult OnPostRenameMedia(Guid? id, string fileName, string newFileName)
    {
        if (id is not { } entryId)
        {
            return BadRequest("Save the entry before managing media.");
        }

        var result = _cdnMediaService.RenameFile(entryId, DateTimeOffset.UtcNow, fileName, newFileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(entryId));
    }

    /// <summary>
    ///     Builds the JSON payload describing an entry's attached media files, in the shape the media manager's
    ///     <c>fetch</c> calls expect.
    /// </summary>
    /// <param name="id">The entry's ID.</param>
    /// <returns>An anonymous object suitable for a <see cref="JsonResult" />.</returns>
    private object MediaListPayload(Guid id)
    {
        var uploaded = _cdnMediaService.ListFiles(id, DateTimeOffset.UtcNow, Area);
        var uploadedNames = uploaded.Select(f => f.FileName).ToHashSet(StringComparer.Ordinal);

        var uploadedEntries = uploaded.Select(f => new
        {
            f.FileName,
            Url = (string?)f.Url,
            Kind = f.Kind.ToString().ToLowerInvariant(),
            SizeBytes = (long?)f.SizeBytes,
            ModifiedAt = (DateTimeOffset?)f.ModifiedAt,
            Missing = false
        });

        var missingEntries = _markdownRenderingService.FindMediaReferences(Input.Body)
            .Where(name => !uploadedNames.Contains(name))
            .Select(name => new
            {
                FileName = name,
                Url = (string?)null,
                Kind = CdnMediaResolver.ResolveMediaKind(name).ToString().ToLowerInvariant(),
                SizeBytes = (long?)null,
                ModifiedAt = (DateTimeOffset?)null,
                Missing = true
            });

        return new { files = uploadedEntries.Concat(missingEntries) };
    }

    /// <summary>
    ///     Builds a save request from the current state of <see cref="Input" />, for either creating an entry or
    ///     saving a new draft of one. The sort order isn't editable from this form - a new entry is appended to the
    ///     end of the display order, and an existing one keeps whatever order it already has (reordering happens
    ///     only from the entry list).
    /// </summary>
    /// <param name="id">The ID of the entry being edited, or <see langword="null" /> if a new entry is being created.</param>
    /// <returns>The built <see cref="SomedayEntrySaveRequest" />.</returns>
    private SomedayEntrySaveRequest BuildSaveRequest(Guid? id)
    {
        var sortOrder = id is { } entryId
            ? _somedayEntryService.GetEntryById(entryId, true).ValueOrDefault?.SortOrder ?? 0
            : _somedayEntryService.GetAllEntries().Count;

        var content = new SomedayEntryDraftContent(Input.Title, Input.Body, Input.Visibility);
        return new SomedayEntrySaveRequest(Input.Slug, sortOrder, content);
    }

    /// <summary>
    ///     Redirects back to this entry's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<SomedayEntry> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a someday entry.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the entry - the completion of "Someday, ...", without that prefix.
        /// </summary>
        /// <value>The title of the entry.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the body of the entry.
        /// </summary>
        /// <value>The body of the entry.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the slug of the entry, used as its anchor ID on the someday page.
        /// </summary>
        /// <value>The slug of the entry.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the visibility of the entry.
        /// </summary>
        /// <value>The visibility of the entry.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;
    }
}
