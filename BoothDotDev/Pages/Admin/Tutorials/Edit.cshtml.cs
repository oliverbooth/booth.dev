using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Tutorials;

/// <summary>
///     Represents the page model for editing a tutorial article in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class Edit : PageModel
{
    private const string Area = "tutorial";
    private readonly CdnMediaService _cdnMediaService;
    private readonly MarkdownRenderingService _markdownRenderingService;

    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="tutorialService">The tutorial service.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    /// <param name="cdnMediaService">The CDN media service.</param>
    public Edit(TutorialService tutorialService, MarkdownRenderingService markdownRenderingService,
        CdnMediaService cdnMediaService)
    {
        _tutorialService = tutorialService;
        _markdownRenderingService = markdownRenderingService;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets or sets the article being edited, if any.
    /// </summary>
    /// <value>The article being edited, or default values if a new article is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new article is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new article is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the article being edited.
    /// </summary>
    /// <value>The ID of the article being edited, or <see langword="null" /> if a new article is being created.</value>
    public Guid? ArticleId { get; private set; }

    /// <summary>
    ///     Gets the full slug path of the article being edited, for the "View article" link.
    /// </summary>
    /// <value>The article's full slug path, or <see langword="null" /> if a new article is being created.</value>
    public string? ArticlePath { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live (published) for this article.
    /// </summary>
    /// <value>The ID of the currently-live draft, or <see langword="null" /> if a new article is being created.</value>
    public Guid? CurrentDraftId { get; private set; }

    /// <summary>
    ///     Gets the article's full draft history, newest first, for the revision history panel.
    /// </summary>
    /// <value>The article's drafts, ordered newest first.</value>
    public IReadOnlyList<TutorialArticleDraft> DraftHistory { get; private set; } = [];

    /// <summary>
    ///     Gets every tutorial folder, for the folder picker.
    /// </summary>
    /// <value>Every tutorial folder, in title order.</value>
    public IReadOnlyList<TutorialFolder> Folders { get; private set; } = [];

    /// <summary>
    ///     Gets every other article, for the next/previous part pickers.
    /// </summary>
    /// <value>Every non-trashed article other than the one being edited.</value>
    public IReadOnlyList<TutorialArticle> OtherArticles { get; private set; } = [];

    /// <summary>
    ///     Gets a value indicating whether the article being edited is trashed.
    /// </summary>
    /// <value><see langword="true" /> if the article is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft currently loaded into the editor.
    /// </summary>
    /// <value>The ID of the draft being viewed, or <see langword="null" /> if a new article is being created.</value>
    public Guid? ViewingDraftId { get; private set; }

    /// <summary>
    ///     Gets the full slug path of the specified folder, for display in the folder picker.
    /// </summary>
    /// <param name="folder">The folder whose path to return.</param>
    /// <returns>The folder's full slug path.</returns>
    public string GetFolderPath(TutorialFolder folder)
    {
        return _tutorialService.GetFullSlug(folder);
    }

    /// <summary>
    ///     Gets the full slug path of the specified article, for display in the next/previous part pickers.
    /// </summary>
    /// <param name="article">The article whose path to return.</param>
    /// <returns>The article's full slug path.</returns>
    public string GetArticlePath(TutorialArticle article)
    {
        return _tutorialService.GetFullSlug(article);
    }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the article to edit. If <see langword="null" />, a new article will be created.</param>
    /// <param name="draftId">
    ///     The ID of a specific draft to view. If <see langword="null" />, the article's newest draft is loaded - not
    ///     necessarily the currently-live one, so reopening the editor resumes from wherever editing was last left
    ///     off rather than silently discarding unpublished draft work.
    /// </param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id, Guid? draftId)
    {
        Folders = _tutorialService.GetAllFolders();

        if (id is null)
        {
            CreatingNew = true;
            OtherArticles = _tutorialService.GetAllArticles();
            Input = new EditModel
            {
                Visibility = Visibility.Published,
                PublishedAt = DateTimeOffset.UtcNow.ToLocalTime(),
                ShowTableOfContents = true,
                TableOfContentsExpanded = true,
                Folder = Folders.FirstOrDefault()?.Id ?? Guid.Empty
            };
            return Page();
        }

        var articleResult = _tutorialService.GetArticle(id.Value, true);
        if (articleResult.IsFailed)
        {
            return NotFound();
        }

        var draftResult = draftId.HasValue
            ? _tutorialService.GetDraft(id.Value, draftId.Value)
            : _tutorialService.GetNewestDraft(id.Value);

        if (draftResult.IsFailed)
        {
            return NotFound();
        }

        var article = articleResult.Value;
        var draft = draftResult.Value;
        ArticleId = article.Id;
        ArticlePath = _tutorialService.GetFullSlug(article);
        CurrentDraftId = article.CurrentDraftId;
        DraftHistory = _tutorialService.GetDraftHistory(id.Value);
        OtherArticles = _tutorialService.GetAllArticles().Where(a => a.Id != article.Id).ToList();
        IsTrashed = article.TrashedAt is not null;
        ViewingDraftId = draft.Id;
        Input = new EditModel
        {
            Title = draft.Title,
            Slug = article.Slug,
            Folder = draft.Folder,
            Rank = draft.Rank,
            Excerpt = draft.Excerpt,
            Body = draft.Body,
            PreviewImageUrl = draft.PreviewImageUrl?.ToString(),
            Visibility = draft.Visibility,
            PublishedAt = article.PublishedAt.ToLocalTime(),
            EnableComments = article.EnableComments,
            ShowTableOfContents = draft.ShowTableOfContents,
            TableOfContentsExpanded = draft.TableOfContentsExpanded,
            NextPart = article.NextPart,
            PreviousPart = article.PreviousPart
        };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving and publishing the article, making it the article's current draft.
    /// </summary>
    /// <param name="id">The ID of the article being edited. If <see langword="null" />, a new article is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;
        ArticleId = id;

        if (!ModelState.IsValid)
        {
            return ReRenderPage();
        }

        var request = BuildSaveRequest();
        var result = id is null
            ? _tutorialService.CreateArticle(request)
            : _tutorialService.PublishArticle(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for saving a draft of the article, without publishing it. The article's
    ///     currently-live draft, if any, is left unchanged.
    /// </summary>
    /// <param name="id">The ID of the article being edited. If <see langword="null" />, a new article is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSaveDraft(Guid? id)
    {
        CreatingNew = id is null;
        ArticleId = id;

        if (!ModelState.IsValid)
        {
            return ReRenderPage();
        }

        var request = BuildSaveRequest();

        // A brand-new article has no prior draft to leave untouched, so its first save - draft or not - always
        // becomes the article's current draft. There's nothing else for it to sensibly point at.
        var result = id is null
            ? _tutorialService.CreateArticle(request)
            : _tutorialService.SaveArticleDraft(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for rendering a live preview of the article's body.
    /// </summary>
    /// <param name="id">The ID of the article being edited. If <see langword="null" />, a new article is being created.</param>
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

        var articleId = id ?? Guid.Empty;
        var html = _markdownRenderingService.Render(Input.Body, articleId, Input.PublishedAt, Area);

        // Tutorials have no font-style concept, so there's no real prose modifier class to send - an empty
        // string keeps content-preview.ts's `prose ${proseClass}` assembly well-formed without touching the TS.
        return new JsonResult(new { html, proseClass = "" });
    }

    /// <summary>
    ///     Handles the POST request for moving the article to the trash.
    /// </summary>
    /// <param name="id">The ID of the article to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid? id)
    {
        if (id is null)
        {
            return BadRequest("Save the article before it can be trashed.");
        }

        ArticleId = id;
        return RedirectOnSuccess(_tutorialService.TrashArticle(id.Value));
    }

    /// <summary>
    ///     Handles the POST request for restoring the article from the trash.
    /// </summary>
    /// <param name="id">The ID of the article to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid? id)
    {
        if (id is null)
        {
            return BadRequest("Save the article before it can be restored.");
        }

        ArticleId = id;
        return RedirectOnSuccess(_tutorialService.RestoreArticle(id.Value));
    }

    /// <summary>
    ///     Handles the POST request for listing the files currently attached to the article via the CDN.
    /// </summary>
    /// <param name="id">The ID of the article being edited. If <see langword="null" />, a new article is being created.</param>
    /// <returns>A JSON payload of the article's attached media files.</returns>
    public IActionResult OnPostListMedia(Guid? id)
    {
        if (id is null)
        {
            return BadRequest("Save the article before managing media.");
        }

        return new JsonResult(MediaListPayload(id.Value));
    }

    /// <summary>
    ///     Handles the POST request for uploading a new file to the article's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the article being edited. If <see langword="null" />, a new article is being created.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of the article's attached media files, including the newly-uploaded one.</returns>
    public async Task<IActionResult> OnPostUploadMediaAsync(Guid? id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return BadRequest("Save the article before managing media.");
        }

        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        var result = await _cdnMediaService.UploadAsync(id.Value, Input.PublishedAt, file, Area, cancellationToken);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(id.Value));
    }

    /// <summary>
    ///     Handles the POST request for deleting a file from the article's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the article being edited. If <see langword="null" />, a new article is being created.</param>
    /// <param name="fileName">The bare filename to delete.</param>
    /// <returns>A JSON payload of the article's remaining attached media files.</returns>
    public IActionResult OnPostDeleteMedia(Guid? id, string fileName)
    {
        if (id is null)
        {
            return BadRequest("Save the article before managing media.");
        }

        var result = _cdnMediaService.DeleteFile(id.Value, Input.PublishedAt, fileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(id.Value));
    }

    /// <summary>
    ///     Handles the POST request for renaming a file in the article's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the article being edited. If <see langword="null" />, a new article is being created.</param>
    /// <param name="fileName">The current bare filename.</param>
    /// <param name="newFileName">The new bare filename. Its extension must match the current one.</param>
    /// <returns>A JSON payload of the article's attached media files, reflecting the rename.</returns>
    public IActionResult OnPostRenameMedia(Guid? id, string fileName, string newFileName)
    {
        if (id is null)
        {
            return BadRequest("Save the article before managing media.");
        }

        var result = _cdnMediaService.RenameFile(id.Value, Input.PublishedAt, fileName, newFileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(id.Value));
    }

    /// <summary>
    ///     Builds the JSON payload describing an article's attached media files, in the shape the media manager's
    ///     <c>fetch</c> calls expect.
    /// </summary>
    /// <param name="id">The article's ID.</param>
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

        var referencedMedia = _markdownRenderingService.FindMediaReferences(Input.Body)
            .Concat(_markdownRenderingService.FindMediaReferences(Input.Excerpt ?? string.Empty));

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
    ///     Builds a save request from the current state of <see cref="Input" />, for either creating an article or
    ///     saving a new draft of one.
    /// </summary>
    /// <returns>The built <see cref="TutorialArticleSaveRequest" />.</returns>
    private TutorialArticleSaveRequest BuildSaveRequest()
    {
        var previewImageUrl = Uri.TryCreate(Input.PreviewImageUrl, UriKind.Absolute, out var uri) ? uri : null;

        var content = new TutorialArticleDraftContent(
            Input.Title,
            Input.Body,
            Input.Excerpt,
            Input.Folder,
            Input.Rank,
            previewImageUrl,
            Input.ShowTableOfContents,
            Input.TableOfContentsExpanded,
            Input.Visibility);

        return new TutorialArticleSaveRequest(
            Input.Slug,
            Input.PublishedAt,
            Input.EnableComments,
            Input.NextPart,
            Input.PreviousPart,
            null,
            content);
    }

    /// <summary>
    ///     Redirects back to this article's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<TutorialArticle> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return ReRenderPage();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Redirects back to this article's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return ReRenderPage();
        }

        return RedirectToPage(new { id = ArticleId });
    }

    /// <summary>
    ///     Re-populates the picker lists and re-renders the form after a failed submission.
    /// </summary>
    /// <returns>The current page, with its picker lists populated.</returns>
    private IActionResult ReRenderPage()
    {
        Folders = _tutorialService.GetAllFolders();
        OtherArticles = ArticleId is { } id
            ? _tutorialService.GetAllArticles().Where(a => a.Id != id).ToList()
            : _tutorialService.GetAllArticles();
        return Page();
    }

    /// <summary>
    ///     Represents the model for editing a tutorial article.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the article.
        /// </summary>
        /// <value>The title of the article.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the slug of the article.
        /// </summary>
        /// <value>The slug of the article.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the ID of the folder the article is contained within.
        /// </summary>
        /// <value>The ID of the folder.</value>
        public Guid Folder { get; set; }

        /// <summary>
        ///     Gets or sets the rank of the article within its folder.
        /// </summary>
        /// <value>The rank.</value>
        public int Rank { get; set; }

        /// <summary>
        ///     Gets or sets the excerpt of the article.
        /// </summary>
        /// <value>The excerpt, or <see langword="null" /> if the article has no excerpt.</value>
        public string? Excerpt { get; set; }

        /// <summary>
        ///     Gets or sets the body of the article.
        /// </summary>
        /// <value>The body of the article.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the URL of the article's preview image.
        /// </summary>
        /// <value>The preview image URL, or <see langword="null" /> if the article has no preview image.</value>
        public string? PreviewImageUrl { get; set; }

        /// <summary>
        ///     Gets or sets the visibility of the article.
        /// </summary>
        /// <value>The visibility of the article.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;

        /// <summary>
        ///     Gets or sets the publication date and time of the article.
        /// </summary>
        /// <value>The publication date and time of the article.</value>
        public DateTimeOffset PublishedAt { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether comments are enabled for the article.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if comments are enabled for the article; otherwise, <see langword="false" />.
        /// </value>
        public bool EnableComments { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to show the table of contents for the article.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the table of contents should be shown; otherwise, <see langword="false" />.
        /// </value>
        public bool ShowTableOfContents { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the table of contents is expanded by default.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the table of contents is expanded by default; otherwise, <see langword="false" />.
        /// </value>
        public bool TableOfContentsExpanded { get; set; } = true;

        /// <summary>
        ///     Gets or sets the ID of the next article to this one, if this article is part of a series.
        /// </summary>
        /// <value>The next part ID.</value>
        public Guid? NextPart { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the previous article to this one, if this article is part of a series.
        /// </summary>
        /// <value>The previous part ID.</value>
        public Guid? PreviousPart { get; set; }
    }
}
