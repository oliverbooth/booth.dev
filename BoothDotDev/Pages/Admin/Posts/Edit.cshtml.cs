using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;

namespace BoothDotDev.Pages.Admin.Posts;

/// <summary>
///     Represents the page model for editing a blog post in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnMediaService.MaxUploadSizeBytes)]
public sealed class Edit : PageModel
{
    private const string Area = "blog";

    private readonly BlogPostService _blogPostService;
    private readonly CdnMediaService _cdnMediaService;
    private readonly MarkdownRenderingService _markdownRenderingService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="cdnMediaService">The CDN media service.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    public Edit(BlogPostService blogPostService, CdnMediaService cdnMediaService, MarkdownRenderingService markdownRenderingService)
    {
        _blogPostService = blogPostService;
        _cdnMediaService = cdnMediaService;
        _markdownRenderingService = markdownRenderingService;
    }

    /// <summary>
    ///     Gets or sets the blog post being edited, if any.
    /// </summary>
    /// <value>The blog post being edited, or <see cref="Option.None{T}" /> if a new post is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new post is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new post is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live (published) for this post.
    /// </summary>
    /// <value>The ID of the currently-live draft, or <see langword="null" /> if a new post is being created.</value>
    public Guid? CurrentDraftId { get; private set; }

    /// <summary>
    ///     Gets the post's full draft history, newest first, for the revision history panel.
    /// </summary>
    /// <value>The post's drafts, ordered newest first.</value>
    public IReadOnlyList<BlogPostDraft> DraftHistory { get; private set; } = [];

    /// <summary>
    ///     Gets the ID of the post being edited.
    /// </summary>
    /// <value>The ID of the post being edited, or <see langword="null" /> if a new post is being created.</value>
    public Guid? PostId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the post being edited is trashed.
    /// </summary>
    /// <value><see langword="true" /> if the post is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Gets the ID of the draft currently loaded into the editor.
    /// </summary>
    /// <value>The ID of the draft being viewed, or <see langword="null" /> if a new post is being created.</value>
    public Guid? ViewingDraftId { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the post to edit. If <see langword="null" />, a new post will be created.</param>
    /// <param name="draftId">
    ///     The ID of a specific draft to view. If <see langword="null" />, the post's newest draft is loaded — not
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
                AuthorId = ResolveAuthorId(),
                EnableComments = true,
                PublishedAt = DateTimeOffset.UtcNow,
                TableOfContentsExpanded = true,
                Visibility = Visibility.Private
            };
            return Page();
        }

        var postResult = _blogPostService.GetPost(id.Value, includeTrashed: true);
        if (postResult.IsFailed)
        {
            return NotFound();
        }

        var draftResult = draftId.HasValue
            ? _blogPostService.GetDraft(id.Value, draftId.Value)
            : _blogPostService.GetNewestDraft(id.Value);

        if (draftResult.IsFailed)
        {
            return NotFound();
        }

        var post = postResult.Value;
        var draft = draftResult.Value;
        PostId = post.Id;
        CurrentDraftId = post.CurrentDraftId;
        DraftHistory = _blogPostService.GetDraftHistory(id.Value);
        IsTrashed = post.TrashedAt is not null;
        ViewingDraftId = draft.Id;
        Input = new EditModel
        {
            AuthorId = post.AuthorId,
            Body = draft.Body,
            EnableComments = post.EnableComments,
            Slug = post.Slug,
            ShowTableOfContents = draft.ShowTableOfContents,
            TableOfContentsExpanded = draft.TableOfContentsExpanded,
            Title = draft.Title,
            Excerpt = draft.Excerpt,
            Tags = string.Join(", ", draft.Tags),
            CategoryId = draft.CategoryId,
            Visibility = draft.Visibility,
            PublishedAt = post.Published
        };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving and publishing the blog post, making it the post's current draft.
    /// </summary>
    /// <param name="id">The ID of the post being edited. If <see langword="null" />, a new post is being created.</param>
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
            ? _blogPostService.CreatePost(request)
            : _blogPostService.PublishPost(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for saving a draft of the blog post, without publishing it. The post's
    ///     currently-live draft, if any, is left unchanged.
    /// </summary>
    /// <param name="id">The ID of the post being edited. If <see langword="null" />, a new post is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSaveDraft(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = BuildSaveRequest();

        // A brand-new post has no prior draft to leave untouched, so its first save — draft or not — always
        // becomes the post's current draft. There's nothing else for it to sensibly point at.
        var result = id is null
            ? _blogPostService.CreatePost(request)
            : _blogPostService.SaveDraft(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for moving the blog post to the trash.
    /// </summary>
    /// <param name="id">The ID of the post to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid? id)
    {
        if (id is not { } postId)
        {
            return BadRequest("Save the post before it can be trashed.");
        }

        return RedirectOnSuccess(_blogPostService.TrashPost(postId));
    }

    /// <summary>
    ///     Handles the POST request for restoring the blog post from the trash.
    /// </summary>
    /// <param name="id">The ID of the post to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid? id)
    {
        if (id is not { } postId)
        {
            return BadRequest("Save the post before it can be restored.");
        }

        return RedirectOnSuccess(_blogPostService.RestorePost(postId));
    }

    /// <summary>
    ///     Handles the POST request for rendering a live preview of the blog post body.
    /// </summary>
    /// <param name="id">The ID of the post being edited. If <see langword="null" />, a new post is being created.</param>
    /// <returns>
    ///     A JSON payload of the rendered preview HTML and the prose CSS class for the post's category. This handler
    ///     backs the editor's live-updating preview pane and is only ever called via <c>fetch</c> — there's no
    ///     server-rendered fallback, since the Markdown editor itself already requires JS to function.
    /// </returns>
    public IActionResult OnPostPreview(Guid? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var html = _markdownRenderingService.Render(
            Input.Body,
            id ?? Guid.Empty,
            Input.PublishedAt,
            area: "blog");

        var category = _blogPostService.GetCategory(Input.CategoryId);
        var proseClass = (category?.FontStyle ?? FontStyle.SansSerif).ToProseClass();

        return new JsonResult(new { html, proseClass });
    }

    /// <summary>
    ///     Handles the POST request for listing the files currently attached to the post via the CDN.
    /// </summary>
    /// <param name="id">The ID of the post being edited. If <see langword="null" />, a new post is being created.</param>
    /// <returns>A JSON payload of the post's attached media files.</returns>
    public IActionResult OnPostListMedia(Guid? id)
    {
        if (id is not { } postId)
        {
            return BadRequest("Save the post before managing media.");
        }

        return new JsonResult(MediaListPayload(postId));
    }

    /// <summary>
    ///     Handles the POST request for uploading a new file to the post's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the post being edited. If <see langword="null" />, a new post is being created.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of the post's attached media files, including the newly-uploaded one.</returns>
    public async Task<IActionResult> OnPostUploadMediaAsync(Guid? id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (id is not { } postId)
        {
            return BadRequest("Save the post before managing media.");
        }

        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        var result = await _cdnMediaService.UploadAsync(postId, Input.PublishedAt, file, Area, cancellationToken);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(postId));
    }

    /// <summary>
    ///     Handles the POST request for deleting a file from the post's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the post being edited. If <see langword="null" />, a new post is being created.</param>
    /// <param name="fileName">The bare filename to delete.</param>
    /// <returns>A JSON payload of the post's remaining attached media files.</returns>
    public IActionResult OnPostDeleteMedia(Guid? id, string fileName)
    {
        if (id is not { } postId)
        {
            return BadRequest("Save the post before managing media.");
        }

        var result = _cdnMediaService.DeleteFile(postId, Input.PublishedAt, fileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(postId));
    }

    /// <summary>
    ///     Handles the POST request for renaming a file in the post's CDN media folder.
    /// </summary>
    /// <param name="id">The ID of the post being edited. If <see langword="null" />, a new post is being created.</param>
    /// <param name="fileName">The current bare filename.</param>
    /// <param name="newFileName">The new bare filename. Its extension must match the current one.</param>
    /// <returns>A JSON payload of the post's attached media files, reflecting the rename.</returns>
    public IActionResult OnPostRenameMedia(Guid? id, string fileName, string newFileName)
    {
        if (id is not { } postId)
        {
            return BadRequest("Save the post before managing media.");
        }

        var result = _cdnMediaService.RenameFile(postId, Input.PublishedAt, fileName, newFileName, Area);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors.Select(e => e.Message));
        }

        return new JsonResult(MediaListPayload(postId));
    }

    /// <summary>
    ///     Builds the JSON payload describing a post's attached media files, in the shape the media manager's
    ///     <c>fetch</c> calls expect.
    /// </summary>
    /// <param name="id">The post's ID.</param>
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
    ///     Splits <see cref="EditModel.Tags" /> into individual tag values.
    /// </summary>
    /// <returns>The individual tag values.</returns>
    private string[] ParseTags()
    {
        return Input.Tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    ///     Builds a save request from the current state of <see cref="Input" />, for either creating a post or
    ///     saving a new draft of one.
    /// </summary>
    /// <returns>The built <see cref="BlogPostSaveRequest" />.</returns>
    private BlogPostSaveRequest BuildSaveRequest()
    {
        var content = new BlogPostDraftContent(Input.Title,
            Input.Body,
            Input.Excerpt,
            Input.CategoryId,
            Input.Visibility,
            ParseTags(),
            Input.ShowTableOfContents,
            Input.TableOfContentsExpanded);

        return new BlogPostSaveRequest(Input.AuthorId, Input.Slug, Input.PublishedAt, Input.EnableComments, content);
    }

    /// <summary>
    ///     Resolves the ID of the currently signed-in admin, for attribution as a post's author.
    /// </summary>
    /// <returns>The ID of the currently signed-in admin.</returns>
    private Guid ResolveAuthorId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var parsedAuthorId) ? parsedAuthorId : Guid.Empty;
    }

    /// <summary>
    ///     Redirects back to this post's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<BlogPost> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a blog post.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the ID of the author attributed to the blog post.
        /// </summary>
        /// <value>The ID of the post's author.</value>
        public Guid AuthorId { get; set; }

        /// <summary>
        ///     Gets or sets the body of the blog post.
        /// </summary>
        /// <value>The body of the blog post.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the ID of the category associated with the blog post.
        /// </summary>
        /// <value>The ID of the category associated with the blog post.</value>
        public Guid CategoryId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether comments are enabled for the blog post.
        /// </summary>
        /// <value><see langword="true" /> if comments are enabled; otherwise, <see langword="false" />.</value>
        public bool EnableComments { get; set; }

        /// <summary>
        ///     Gets or sets the excerpt of the blog post.
        /// </summary>
        /// <value>The excerpt of the blog post.</value>
        public string? Excerpt { get; set; }

        /// <summary>
        ///     Gets or sets the publication date and time of the blog post.
        /// </summary>
        /// <value>The publication date and time of the blog post.</value>
        public DateTimeOffset PublishedAt { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to show the table of contents for the blog post.
        /// </summary>
        /// <value><see langword="true" /> if the table of contents should be shown; otherwise, <see langword="false" />.</value>
        public bool ShowTableOfContents { get; set; }

        /// <summary>
        ///     Gets or sets the URL slug of the blog post.
        /// </summary>
        /// <value>The URL slug of the blog post.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a value indicating whether the table of contents is expanded by default.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the table of contents is expanded by default; otherwise, <see langword="false" />.
        /// </value>
        public bool TableOfContentsExpanded { get; set; }

        /// <summary>
        ///     Gets or sets the tags associated with the blog post.
        /// </summary>
        /// <value>The tags associated with the blog post.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Tags { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the title of the blog post.
        /// </summary>
        /// <value>The title of the blog post.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the visibility of the blog post.
        /// </summary>
        /// <value>The visibility of the blog post.</value>
        public Visibility Visibility { get; set; } = Visibility.None;
    }
}
