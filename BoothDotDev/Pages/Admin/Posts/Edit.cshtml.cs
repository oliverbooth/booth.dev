using BoothDotDev.Data;
using BoothDotDev.Extensions;
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
public sealed class Edit : PageModel
{
    private readonly BlogPostService _blogPostService;
    private readonly MarkdownRenderingService _markdownRenderingService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    public Edit(BlogPostService blogPostService, MarkdownRenderingService markdownRenderingService)
    {
        _blogPostService = blogPostService;
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
            Input = new EditModel { PublishedAt = DateTimeOffset.Now, Visibility = Visibility.Private };
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
    ///     Represents the model for editing a blog post.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the body of the blog post.
        /// </summary>
        /// <value>The body of the blog post.</value>
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
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the tags associated with the blog post.
        /// </summary>
        /// <value>The tags associated with the blog post.</value>
        public string Tags { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the title of the blog post.
        /// </summary>
        /// <value>The title of the blog post.</value>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the visibility of the blog post.
        /// </summary>
        /// <value>The visibility of the blog post.</value>
        public Visibility Visibility { get; set; } = Visibility.None;
    }
}
