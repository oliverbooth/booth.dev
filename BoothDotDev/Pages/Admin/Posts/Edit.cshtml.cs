using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BoothDotDev.Data;
using BoothDotDev.Extensions;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
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
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the post to edit. If <see langword="null" />, a new post will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            Input = new EditModel { PublishedAt = DateTimeOffset.UtcNow, Visibility = Visibility.Private };
            return Page();
        }

        var result = _blogPostService.GetPost(id.Value);
        if (result.IsSuccess)
        {
            var post = result.Value;
            Input = new EditModel
            {
                Body = post.Body,
                Slug = post.Slug,
                Title = post.Title,
                Excerpt = post.Excerpt,
                Tags = string.Join(", ", post.Tags),
                CategoryId = post.CategoryId,
                Visibility = post.Visibility,
                PublishedAt = post.Published
            };
        }
        else
        {
            return NotFound();
        }

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the blog post.
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

        var tags = Input.Tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var authorId = Guid.TryParse(claim, out var parsedAuthorId) ? parsedAuthorId : Guid.Empty;

        var result = id is null
            ? _blogPostService.CreatePost(authorId, Input.Title, Input.Slug, Input.Body, Input.Excerpt,
                Input.CategoryId, Input.Visibility, Input.PublishedAt, tags)
            : _blogPostService.UpdatePost(id.Value, Input.Title, Input.Slug, Input.Body, Input.Excerpt,
                Input.CategoryId, Input.Visibility, Input.PublishedAt, tags);

        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
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
    ///     Represents the model for editing a blog post.
    /// </summary>
    public sealed class EditModel
    {
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
        ///     Gets or sets the URL slug of the blog post.
        /// </summary>
        /// <value>The URL slug of the blog post.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

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
