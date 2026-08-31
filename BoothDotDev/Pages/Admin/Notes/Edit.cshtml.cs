using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Notes;

using Note = Data.Models.Note;

/// <summary>
///     Represents the page model for editing a note in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class Edit : PageModel
{
    private const string Area = "note";

    private readonly NoteService _noteService;
    private readonly MarkdownRenderingService _markdownRenderingService;
    private readonly CdnMediaService _cdnMediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="noteService">The note service.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    /// <param name="cdnMediaService">The CDN media service.</param>
    public Edit(NoteService noteService, MarkdownRenderingService markdownRenderingService, CdnMediaService cdnMediaService)
    {
        _noteService = noteService;
        _markdownRenderingService = markdownRenderingService;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets or sets the note being edited, if any.
    /// </summary>
    /// <value>The note being edited, or default values if a new note is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new note is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new note is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live (published) for this note.
    /// </summary>
    /// <value>The ID of the currently-live draft, or <see langword="null" /> if a new note is being created.</value>
    public Guid? CurrentDraftId { get; private set; }

    /// <summary>
    ///     Gets the note's full draft history, newest first, for the revision history panel.
    /// </summary>
    /// <value>The note's drafts, ordered newest first.</value>
    public IReadOnlyList<NoteDraft> DraftHistory { get; private set; } = [];

    /// <summary>
    ///     Gets the ID of the note being edited.
    /// </summary>
    /// <value>The ID of the note being edited, or <see langword="null" /> if a new note is being created.</value>
    public Guid? NoteId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the note being edited is trashed.
    /// </summary>
    /// <value><see langword="true" /> if the note is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft currently loaded into the editor.
    /// </summary>
    /// <value>The ID of the draft being viewed, or <see langword="null" /> if a new note is being created.</value>
    public Guid? ViewingDraftId { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the note to edit. If <see langword="null" />, a new note will be created.</param>
    /// <param name="draftId">
    ///     The ID of a specific draft to view. If <see langword="null" />, the note's newest draft is loaded - not
    ///     necessarily the currently-live one, so reopening the editor resumes from wherever editing was last left
    ///     off rather than silently discarding unpublished draft work.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id, Guid? draftId)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            Input = new EditModel
            {
                Visibility = Visibility.Published,
                FontStyle = FontStyle.Serif,
                PublishedAt = DateTimeOffset.UtcNow.ToLocalTime()
            };
            return Page();
        }

        var noteResult = _noteService.GetNoteById(id.Value, includeTrashed: true);
        if (noteResult.IsFailed)
        {
            return NotFound();
        }

        var draftResult = draftId.HasValue
            ? _noteService.GetDraft(id.Value, draftId.Value)
            : _noteService.GetNewestDraft(id.Value);

        if (draftResult.IsFailed)
        {
            return NotFound();
        }

        var note = noteResult.Value;
        var draft = draftResult.Value;
        NoteId = note.Id;
        CurrentDraftId = note.CurrentDraftId;
        DraftHistory = _noteService.GetDraftHistory(id.Value);
        IsTrashed = note.TrashedAt is not null;
        ViewingDraftId = draft.Id;
        Input = new EditModel
        {
            Title = draft.Title,
            Content = draft.Content,
            FontStyle = draft.FontStyle,
            Visibility = draft.Visibility,
            PublishedAt = note.PublishedAt.ToLocalTime()
        };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving and publishing the note, making it the note's current draft.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = BuildSaveRequest();
        var result = id is null
            ? _noteService.CreateNote(request)
            : _noteService.PublishNote(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for saving a draft of the note, without publishing it. The note's
    ///     currently-live draft, if any, is left unchanged.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSaveDraft(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = BuildSaveRequest();

        // A brand-new note has no prior draft to leave untouched, so its first save - draft or not - always
        // becomes the note's current draft. There's nothing else for it to sensibly point at.
        var result = id is null
            ? _noteService.CreateNote(request)
            : _noteService.SaveDraft(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for rendering a live preview of the note content.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <returns>
    ///     A JSON payload of the rendered preview HTML and the prose CSS class for the note's font style. This
    ///     handler backs the editor's live-updating preview pane and is only ever called via <c>fetch</c> - there's
    ///     no server-rendered fallback, since the Markdown editor itself already requires JS to function.
    /// </returns>
    public IActionResult OnPostPreview(Guid? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var html = _markdownRenderingService.Render(Input.Content, id ?? Guid.Empty, Input.PublishedAt, Area);
        var proseClass = Input.FontStyle.ToProseClass();

        return new JsonResult(new { html, proseClass });
    }

    /// <summary>
    ///     Handles the POST request for moving the note to the trash.
    /// </summary>
    /// <param name="id">The ID of the note to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid? id)
    {
        if (id is not { } noteId)
        {
            return BadRequest("Save the note before it can be trashed.");
        }

        return RedirectOnSuccess(_noteService.TrashNote(noteId));
    }

    /// <summary>
    ///     Handles the POST request for restoring the note from the trash.
    /// </summary>
    /// <param name="id">The ID of the note to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid? id)
    {
        if (id is not { } noteId)
        {
            return BadRequest("Save the note before it can be restored.");
        }

        return RedirectOnSuccess(_noteService.RestoreNote(noteId));
    }

    /// <summary>
    ///     Handles the POST request for listing the files currently attached to the note via the CDN.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <returns>A JSON payload of the note's attached media files.</returns>
    public IActionResult OnPostListMedia(Guid? id)
    {
        if (id is not { } noteId)
        {
            return BadRequest("Save the note before managing media.");
        }

        return new JsonResult(MediaListPayload(noteId));
    }

    /// <summary>
    ///     Handles the POST request for uploading a new file to the note's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of the note's attached media files, including the newly-uploaded one.</returns>
    public async Task<IActionResult> OnPostUploadMediaAsync(Guid? id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (id is not { } noteId)
        {
            return BadRequest("Save the note before managing media.");
        }

        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        var result = await _cdnMediaService.UploadAsync(noteId, Input.PublishedAt, file, Area, cancellationToken);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(noteId));
    }

    /// <summary>
    ///     Handles the POST request for deleting a file from the note's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <param name="fileName">The bare filename to delete.</param>
    /// <returns>A JSON payload of the note's remaining attached media files.</returns>
    public IActionResult OnPostDeleteMedia(Guid? id, string fileName)
    {
        if (id is not { } noteId)
        {
            return BadRequest("Save the note before managing media.");
        }

        var result = _cdnMediaService.DeleteFile(noteId, Input.PublishedAt, fileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(noteId));
    }

    /// <summary>
    ///     Handles the POST request for renaming a file in the note's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the note being edited. If <see langword="null" />, a new note is being created.</param>
    /// <param name="fileName">The current bare filename.</param>
    /// <param name="newFileName">The new bare filename. Its extension must match the current one.</param>
    /// <returns>A JSON payload of the note's attached media files, reflecting the rename.</returns>
    public IActionResult OnPostRenameMedia(Guid? id, string fileName, string newFileName)
    {
        if (id is not { } noteId)
        {
            return BadRequest("Save the note before managing media.");
        }

        var result = _cdnMediaService.RenameFile(noteId, Input.PublishedAt, fileName, newFileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(noteId));
    }

    /// <summary>
    ///     Builds the JSON payload describing a note's attached media files, in the shape the media manager's
    ///     <c>fetch</c> calls expect.
    /// </summary>
    /// <param name="id">The note's ID.</param>
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

        var missingEntries = _markdownRenderingService.FindMediaReferences(Input.Content)
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
    ///     Builds a save request from the current state of <see cref="Input" />, for either creating a note or
    ///     saving a new draft of one.
    /// </summary>
    /// <returns>The built <see cref="NoteSaveRequest" />.</returns>
    private NoteSaveRequest BuildSaveRequest()
    {
        var content = new NoteDraftContent(Input.Title, Input.Content, Input.FontStyle, Input.Visibility);
        return new NoteSaveRequest(Input.PublishedAt, content);
    }

    /// <summary>
    ///     Redirects back to this note's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<Note> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a note.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the note.
        /// </summary>
        /// <value>The title of the note.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the content of the note.
        /// </summary>
        /// <value>The content of the note.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the font style of the note.
        /// </summary>
        /// <value>The font style of the note.</value>
        public FontStyle FontStyle { get; set; } = FontStyle.Serif;

        /// <summary>
        ///     Gets or sets the visibility of the note.
        /// </summary>
        /// <value>The visibility of the note.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;

        /// <summary>
        ///     Gets or sets the publication date and time of the note.
        /// </summary>
        /// <value>The publication date and time of the note.</value>
        public DateTimeOffset PublishedAt { get; set; }
    }
}
