using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
using DEDrake;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Challenges;

using DevChallenge = Data.Models.DevChallenge;

/// <summary>
///     Represents the page model for editing a challenge in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class Edit : PageModel
{
    private const string Area = "challenge";

    private readonly DevChallengeService _devChallengeService;
    private readonly MarkdownRenderingService _markdownRenderingService;
    private readonly CdnMediaService _cdnMediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="devChallengeService">The dev challenge service.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    /// <param name="cdnMediaService">The CDN media service.</param>
    public Edit(DevChallengeService devChallengeService, MarkdownRenderingService markdownRenderingService, CdnMediaService cdnMediaService)
    {
        _devChallengeService = devChallengeService;
        _markdownRenderingService = markdownRenderingService;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets or sets the challenge being edited, if any.
    /// </summary>
    /// <value>The challenge being edited, or default values if a new challenge is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new challenge is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new challenge is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the challenge being edited.
    /// </summary>
    /// <value>The ID of the challenge being edited, or <see langword="null" /> if a new challenge is being created.</value>
    public ShortGuid? ChallengeId { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live (published) for this challenge.
    /// </summary>
    /// <value>The ID of the currently-live draft, or <see langword="null" /> if a new challenge is being created.</value>
    public Guid? CurrentDraftId { get; private set; }

    /// <summary>
    ///     Gets the challenge's full draft history, newest first, for the revision history panel.
    /// </summary>
    /// <value>The challenge's drafts, ordered newest first.</value>
    public IReadOnlyList<Data.Models.DevChallengeDraft> DraftHistory { get; private set; } = [];

    /// <summary>
    ///     Gets a value indicating whether the challenge being edited is trashed.
    /// </summary>
    /// <value><see langword="true" /> if the challenge is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft currently loaded into the editor.
    /// </summary>
    /// <value>The ID of the draft being viewed, or <see langword="null" /> if a new challenge is being created.</value>
    public Guid? ViewingDraftId { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the challenge to edit. If <see langword="null" />, a new challenge will be created.</param>
    /// <param name="draftId">
    ///     The ID of a specific draft to view. If <see langword="null" />, the challenge's newest draft is loaded — not
    ///     necessarily the currently-live one, so reopening the editor resumes from wherever editing was last left
    ///     off rather than silently discarding unpublished draft work.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(string? id, Guid? draftId)
    {
        if (id is null)
        {
            CreatingNew = true;
            Input = new EditModel
            {
                Visibility = Visibility.Published,
                PublishedAt = DateTimeOffset.UtcNow.ToLocalTime()
            };
            return Page();
        }

        ShortGuid challengeId;

        try
        {
            challengeId = ShortGuid.Parse(id);
        }
        catch (FormatException)
        {
            return NotFound();
        }

        var challengeResult = _devChallengeService.GetChallengeById(challengeId, includeTrashed: true);
        if (challengeResult.IsFailed)
        {
            return NotFound();
        }

        var draftResult = draftId.HasValue
            ? _devChallengeService.GetDraft(challengeId, draftId.Value)
            : _devChallengeService.GetNewestDraft(challengeId);

        if (draftResult.IsFailed)
        {
            return NotFound();
        }

        var challenge = challengeResult.Value;
        var draft = draftResult.Value;
        ChallengeId = challenge.Id;
        CurrentDraftId = challenge.CurrentDraftId;
        DraftHistory = _devChallengeService.GetDraftHistory(challengeId);
        IsTrashed = challenge.TrashedAt is not null;
        ViewingDraftId = draft.Id;
        Input = new EditModel
        {
            Title = draft.Title,
            Description = draft.Description,
            Solution = draft.Solution,
            ShowSolution = draft.ShowSolution,
            Visibility = draft.Visibility,
            PublishedAt = challenge.PublishedAt.ToLocalTime()
        };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving and publishing the challenge, making it the challenge's current draft.
    /// </summary>
    /// <param name="id">The ID of the challenge being edited. If <see langword="null" />, a new challenge is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(string? id)
    {
        CreatingNew = id is null;
        ChallengeId = id is not null ? ShortGuid.Parse(id) : (ShortGuid?)null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = BuildSaveRequest();
        var result = id is null
            ? _devChallengeService.CreateChallenge(request)
            : _devChallengeService.PublishChallenge(ShortGuid.Parse(id), request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for saving a draft of the challenge, without publishing it. The challenge's
    ///     currently-live draft, if any, is left unchanged.
    /// </summary>
    /// <param name="id">The ID of the challenge being edited. If <see langword="null" />, a new challenge is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSaveDraft(string? id)
    {
        CreatingNew = id is null;
        ChallengeId = id is not null ? ShortGuid.Parse(id) : (ShortGuid?)null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = BuildSaveRequest();

        // A brand-new challenge has no prior draft to leave untouched, so its first save — draft or not —
        // always becomes the challenge's current draft. There's nothing else for it to sensibly point at.
        var result = id is null
            ? _devChallengeService.CreateChallenge(request)
            : _devChallengeService.SaveDraft(ShortGuid.Parse(id), request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for rendering a live preview of the challenge's description or solution.
    /// </summary>
    /// <param name="id">The ID of the challenge being edited. If <see langword="null" />, a new challenge is being created.</param>
    /// <param name="field">
    ///     Which field to render: <c>"solution"</c> for the solution, or anything else (including <see langword="null" />)
    ///     for the description. Sent by the preview pane's field tabs.
    /// </param>
    /// <returns>
    ///     A JSON payload of the rendered preview HTML. This handler backs the editor's live-updating preview pane
    ///     and is only ever called via <c>fetch</c> — there's no server-rendered fallback, since the Markdown editor
    ///     itself already requires JS to function.
    /// </returns>
    public IActionResult OnPostPreview(string? id, string? field)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var challengeGuid = id is not null ? (Guid)ShortGuid.Parse(id) : Guid.Empty;
        var markdown = field == "solution" ? Input.Solution ?? string.Empty : Input.Description;
        var html = _markdownRenderingService.Render(markdown, challengeGuid, Input.PublishedAt, Area);

        // Challenges have no font-style concept, so there's no real prose modifier class to send - an empty
        // string keeps content-preview.ts's `prose ${proseClass}` assembly well-formed without touching the TS.
        return new JsonResult(new { html, proseClass = "" });
    }

    /// <summary>
    ///     Handles the POST request for moving the challenge to the trash.
    /// </summary>
    /// <param name="id">The ID of the challenge to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(string? id)
    {
        if (id is null)
        {
            return BadRequest("Save the challenge before it can be trashed.");
        }

        ChallengeId = ShortGuid.Parse(id);
        return RedirectOnSuccess(_devChallengeService.TrashChallenge(ChallengeId.Value));
    }

    /// <summary>
    ///     Handles the POST request for restoring the challenge from the trash.
    /// </summary>
    /// <param name="id">The ID of the challenge to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(string? id)
    {
        if (id is null)
        {
            return BadRequest("Save the challenge before it can be restored.");
        }

        ChallengeId = ShortGuid.Parse(id);
        return RedirectOnSuccess(_devChallengeService.RestoreChallenge(ChallengeId.Value));
    }

    /// <summary>
    ///     Handles the POST request for listing the files currently attached to the challenge via the CDN.
    /// </summary>
    /// <param name="id">The ID of the challenge being edited. If <see langword="null" />, a new challenge is being created.</param>
    /// <returns>A JSON payload of the challenge's attached media files.</returns>
    public IActionResult OnPostListMedia(string? id)
    {
        if (id is null)
        {
            return BadRequest("Save the challenge before managing media.");
        }

        return new JsonResult(MediaListPayload(ShortGuid.Parse(id)));
    }

    /// <summary>
    ///     Handles the POST request for uploading a new file to the challenge's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the challenge being edited. If <see langword="null" />, a new challenge is being created.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of the challenge's attached media files, including the newly-uploaded one.</returns>
    public async Task<IActionResult> OnPostUploadMediaAsync(string? id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return BadRequest("Save the challenge before managing media.");
        }

        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        var challengeId = ShortGuid.Parse(id);
        var result = await _cdnMediaService.UploadAsync(challengeId, Input.PublishedAt, file, Area, cancellationToken);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(challengeId));
    }

    /// <summary>
    ///     Handles the POST request for deleting a file from the challenge's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the challenge being edited. If <see langword="null" />, a new challenge is being created.</param>
    /// <param name="fileName">The bare filename to delete.</param>
    /// <returns>A JSON payload of the challenge's remaining attached media files.</returns>
    public IActionResult OnPostDeleteMedia(string? id, string fileName)
    {
        if (id is null)
        {
            return BadRequest("Save the challenge before managing media.");
        }

        var challengeId = ShortGuid.Parse(id);
        var result = _cdnMediaService.DeleteFile(challengeId, Input.PublishedAt, fileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(challengeId));
    }

    /// <summary>
    ///     Handles the POST request for renaming a file in the challenge's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the challenge being edited. If <see langword="null" />, a new challenge is being created.</param>
    /// <param name="fileName">The current bare filename.</param>
    /// <param name="newFileName">The new bare filename. Its extension must match the current one.</param>
    /// <returns>A JSON payload of the challenge's attached media files, reflecting the rename.</returns>
    public IActionResult OnPostRenameMedia(string? id, string fileName, string newFileName)
    {
        if (id is null)
        {
            return BadRequest("Save the challenge before managing media.");
        }

        var challengeId = ShortGuid.Parse(id);
        var result = _cdnMediaService.RenameFile(challengeId, Input.PublishedAt, fileName, newFileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(challengeId));
    }

    /// <summary>
    ///     Builds the JSON payload describing a challenge's attached media files, in the shape the media manager's
    ///     <c>fetch</c> calls expect.
    /// </summary>
    /// <param name="id">The challenge's ID.</param>
    /// <returns>An anonymous object suitable for a <see cref="JsonResult" />.</returns>
    private object MediaListPayload(ShortGuid id)
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

        var referencedMedia = _markdownRenderingService.FindMediaReferences(Input.Description)
            .Concat(_markdownRenderingService.FindMediaReferences(Input.Solution ?? string.Empty));

        var missingEntries = referencedMedia.Distinct(StringComparer.Ordinal)
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
    ///     Builds a save request from the current state of <see cref="Input" />, for either creating a challenge or
    ///     saving a new draft of one.
    /// </summary>
    /// <returns>The built <see cref="DevChallengeSaveRequest" />.</returns>
    private DevChallengeSaveRequest BuildSaveRequest()
    {
        var content = new DevChallengeDraftContent(Input.Title, Input.Description, Input.Solution, Input.ShowSolution, Input.Visibility);
        return new DevChallengeSaveRequest(Input.PublishedAt, content);
    }

    /// <summary>
    ///     Redirects back to this challenge's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<DevChallenge> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a challenge.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the challenge.
        /// </summary>
        /// <value>The title of the challenge.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the description of the challenge.
        /// </summary>
        /// <value>The description of the challenge.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the solution for the challenge.
        /// </summary>
        /// <value>The solution for the challenge.</value>
        public string? Solution { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the solution should be shown.
        /// </summary>
        /// <value><see langword="true" /> if the solution should be shown; otherwise, <see langword="false" />.</value>
        public bool ShowSolution { get; set; }

        /// <summary>
        ///     Gets or sets the visibility of the challenge.
        /// </summary>
        /// <value>The visibility of the challenge.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;

        /// <summary>
        ///     Gets or sets the publication date and time of the challenge.
        /// </summary>
        /// <value>The publication date and time of the challenge.</value>
        public DateTimeOffset PublishedAt { get; set; }
    }
}
