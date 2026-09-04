using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Projects.Devlogs;

/// <summary>
///     Represents the page model for editing a project devlog entry in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class Edit : PageModel
{
    private const string Area = "devlog";
    private readonly CdnMediaService _cdnMediaService;
    private readonly MarkdownRenderingService _markdownRenderingService;

    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="projectService">The project service.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    /// <param name="cdnMediaService">The CDN media service.</param>
    public Edit(ProjectService projectService, MarkdownRenderingService markdownRenderingService, CdnMediaService cdnMediaService)
    {
        _projectService = projectService;
        _markdownRenderingService = markdownRenderingService;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets or sets the devlog entry being edited, if any.
    /// </summary>
    /// <value>The devlog entry being edited, or default values if a new entry is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new devlog entry is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new entry is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the project this devlog entry belongs to.
    /// </summary>
    /// <value>The project.</value>
    public Project Project { get; private set; } = null!;

    /// <summary>
    ///     Gets the ID of the draft that is currently live (published) for this devlog entry.
    /// </summary>
    /// <value>The ID of the currently-live draft, or <see langword="null" /> if a new entry is being created.</value>
    public Guid? CurrentDraftId { get; private set; }

    /// <summary>
    ///     Gets the devlog entry's full draft history, newest first, for the revision history panel.
    /// </summary>
    /// <value>The entry's drafts, ordered newest first.</value>
    public IReadOnlyList<ProjectDevlogDraft> DraftHistory { get; private set; } = [];

    /// <summary>
    ///     Gets the ID of the devlog entry being edited.
    /// </summary>
    /// <value>The ID of the entry being edited, or <see langword="null" /> if a new entry is being created.</value>
    public Guid? DevlogId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the devlog entry being edited is trashed.
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
    /// <param name="projectId">The ID of the project this devlog entry belongs to.</param>
    /// <param name="id">The ID of the devlog entry to edit. If <see langword="null" />, a new entry will be created.</param>
    /// <param name="draftId">
    ///     The ID of a specific draft to view. If <see langword="null" />, the entry's newest draft is loaded - not
    ///     necessarily the currently-live one, so reopening the editor resumes from wherever editing was last left
    ///     off rather than silently discarding unpublished draft work.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid projectId, Guid? id, Guid? draftId)
    {
        var projectResult = _projectService.GetProject(projectId);
        if (projectResult.IsFailed)
        {
            return NotFound();
        }

        Project = projectResult.Value;

        if (!id.HasValue)
        {
            CreatingNew = true;
            Input = new EditModel
            {
                Visibility = Visibility.Published, EnableComments = true, PublishedAt = DateTimeOffset.UtcNow.ToLocalTime()
            };
            return Page();
        }

        var devlogResult = _projectService.GetDevlogById(id.Value, true);
        if (devlogResult.IsFailed)
        {
            return NotFound();
        }

        var draftResult = draftId.HasValue
            ? _projectService.GetDraft(id.Value, draftId.Value)
            : _projectService.GetNewestDraft(id.Value);

        if (draftResult.IsFailed)
        {
            return NotFound();
        }

        var devlog = devlogResult.Value;
        var draft = draftResult.Value;
        DevlogId = devlog.Id;
        CurrentDraftId = devlog.CurrentDraftId;
        DraftHistory = _projectService.GetDraftHistory(id.Value);
        IsTrashed = devlog.TrashedAt is not null;
        ViewingDraftId = draft.Id;
        Input = new EditModel
        {
            Title = draft.Title,
            Body = draft.Body,
            Visibility = draft.Visibility,
            Slug = devlog.Slug,
            EnableComments = devlog.EnableComments,
            PublishedAt = devlog.PublishedAt.ToLocalTime()
        };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving and publishing the devlog entry, making it the entry's current
    ///     draft.
    /// </summary>
    /// <param name="projectId">The ID of the project this devlog entry belongs to.</param>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid projectId, Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return WithProject(projectId, Page());
        }

        var request = BuildSaveRequest(projectId);
        var result = id is null
            ? _projectService.CreateDevlog(request)
            : _projectService.PublishDevlog(id.Value, request);

        return RedirectOnSuccess(projectId, result);
    }

    /// <summary>
    ///     Handles the POST request for saving a draft of the devlog entry, without publishing it. The entry's
    ///     currently-live draft, if any, is left unchanged.
    /// </summary>
    /// <param name="projectId">The ID of the project this devlog entry belongs to.</param>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSaveDraft(Guid projectId, Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return WithProject(projectId, Page());
        }

        var request = BuildSaveRequest(projectId);

        // A brand-new devlog entry has no prior draft to leave untouched, so its first save - draft or not -
        // always becomes the entry's current draft. There's nothing else for it to sensibly point at.
        var result = id is null
            ? _projectService.CreateDevlog(request)
            : _projectService.SaveDevlogDraft(id.Value, request);

        return RedirectOnSuccess(projectId, result);
    }

    /// <summary>
    ///     Handles the POST request for rendering a live preview of the devlog entry's body.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>A JSON payload of the rendered preview HTML.</returns>
    public IActionResult OnPostPreview(Guid? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var html = _markdownRenderingService.Render(Input.Body, id ?? Guid.Empty, Input.PublishedAt, Area);

        // Devlog entries have no font-style concept, so there's no real prose modifier class to send - an
        // empty string keeps content-preview.ts's `prose ${proseClass}` assembly well-formed without touching
        // the TS.
        return new JsonResult(new { html, proseClass = "" });
    }

    /// <summary>
    ///     Handles the POST request for moving the devlog entry to the trash.
    /// </summary>
    /// <param name="projectId">The ID of the project this devlog entry belongs to.</param>
    /// <param name="id">The ID of the entry to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid projectId, Guid? id)
    {
        if (id is not { } devlogId)
        {
            return BadRequest("Save the devlog entry before it can be trashed.");
        }

        return RedirectOnSuccess(projectId, _projectService.TrashDevlog(devlogId));
    }

    /// <summary>
    ///     Handles the POST request for restoring the devlog entry from the trash.
    /// </summary>
    /// <param name="projectId">The ID of the project this devlog entry belongs to.</param>
    /// <param name="id">The ID of the entry to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid projectId, Guid? id)
    {
        if (id is not { } devlogId)
        {
            return BadRequest("Save the devlog entry before it can be restored.");
        }

        return RedirectOnSuccess(projectId, _projectService.RestoreDevlog(devlogId));
    }

    /// <summary>
    ///     Handles the POST request for listing the files currently attached to the devlog entry via the CDN.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>A JSON payload of the entry's attached media files.</returns>
    public IActionResult OnPostListMedia(Guid? id)
    {
        if (id is not { } devlogId)
        {
            return BadRequest("Save the devlog entry before managing media.");
        }

        return new JsonResult(MediaListPayload(devlogId));
    }

    /// <summary>
    ///     Handles the POST request for uploading a new file to the devlog entry's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of the entry's attached media files, including the newly-uploaded one.</returns>
    public async Task<IActionResult> OnPostUploadMediaAsync(Guid? id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (id is not { } devlogId)
        {
            return BadRequest("Save the devlog entry before managing media.");
        }

        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        var result = await _cdnMediaService.UploadAsync(devlogId, Input.PublishedAt, file, Area, cancellationToken);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(devlogId));
    }

    /// <summary>
    ///     Handles the POST request for deleting a file from the devlog entry's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <param name="fileName">The bare filename to delete.</param>
    /// <returns>A JSON payload of the entry's remaining attached media files.</returns>
    public IActionResult OnPostDeleteMedia(Guid? id, string fileName)
    {
        if (id is not { } devlogId)
        {
            return BadRequest("Save the devlog entry before managing media.");
        }

        var result = _cdnMediaService.DeleteFile(devlogId, Input.PublishedAt, fileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(devlogId));
    }

    /// <summary>
    ///     Handles the POST request for renaming a file in the devlog entry's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <param name="fileName">The current bare filename.</param>
    /// <param name="newFileName">The new bare filename. Its extension must match the current one.</param>
    /// <returns>A JSON payload of the entry's attached media files, reflecting the rename.</returns>
    public IActionResult OnPostRenameMedia(Guid? id, string fileName, string newFileName)
    {
        if (id is not { } devlogId)
        {
            return BadRequest("Save the devlog entry before managing media.");
        }

        var result = _cdnMediaService.RenameFile(devlogId, Input.PublishedAt, fileName, newFileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(devlogId));
    }

    /// <summary>
    ///     Builds the JSON payload describing a devlog entry's attached media files, in the shape the media
    ///     manager's <c>fetch</c> calls expect.
    /// </summary>
    /// <param name="id">The devlog entry's ID.</param>
    /// <returns>An anonymous object suitable for a <see cref="JsonResult" />.</returns>
    private object MediaListPayload(Guid id)
    {
        var uploaded = _cdnMediaService.ListFiles(id, Input.PublishedAt, Area);
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
    ///     Populates <see cref="Project" /> and returns the given result, for re-rendering the form after a
    ///     validation failure.
    /// </summary>
    private IActionResult WithProject(Guid projectId, IActionResult result)
    {
        if (_projectService.TryGetProject(projectId, out var project))
        {
            Project = project;
        }

        return result;
    }

    /// <summary>
    ///     Builds a save request from the current state of <see cref="Input" />, for either creating a devlog entry
    ///     or saving a new draft of one.
    /// </summary>
    /// <returns>The built <see cref="ProjectDevlogSaveRequest" />.</returns>
    private ProjectDevlogSaveRequest BuildSaveRequest(Guid projectId)
    {
        var content = new ProjectDevlogDraftContent(Input.Title, Input.Body, Input.Visibility);
        return new ProjectDevlogSaveRequest(projectId, Input.Slug, Input.PublishedAt, Input.EnableComments, content);
    }

    /// <summary>
    ///     Redirects back to this devlog entry's edit page on success, or re-renders the form with an error on
    ///     failure.
    /// </summary>
    /// <param name="projectId">The ID of the project this devlog entry belongs to.</param>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Guid projectId, Result<ProjectDevlog> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return WithProject(projectId, Page());
        }

        return RedirectToPage(new { projectId, id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a devlog entry.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the devlog entry.
        /// </summary>
        /// <value>The title.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the slug of the devlog entry.
        /// </summary>
        /// <value>The slug.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the body of the devlog entry.
        /// </summary>
        /// <value>The body.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the visibility of the devlog entry.
        /// </summary>
        /// <value>The visibility.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;

        /// <summary>
        ///     Gets or sets a value indicating whether comments are enabled for the devlog entry.
        /// </summary>
        /// <value><see langword="true" /> if comments are enabled; otherwise, <see langword="false" />.</value>
        public bool EnableComments { get; set; }

        /// <summary>
        ///     Gets or sets the publication date and time of the devlog entry.
        /// </summary>
        /// <value>The publication date and time.</value>
        public DateTimeOffset PublishedAt { get; set; }
    }
}
