using BoothDotDev.Data;
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    public Edit(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the blog post being edited, if any.
    /// </summary>
    /// <value>The blog post being edited, or <see cref="Option.None{T}" /> if a new post is being created.</value>
    public EditModel CurrentModel { get; private set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new post is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new post is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the post to edit. If <see langword="null" />, a new post will be created.</param>
    public IActionResult OnGet(Guid? id)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            CurrentModel = new EditModel { PublishedAt = DateTimeOffset.Now, Visibility = Visibility.Private };
            return Page();
        }

        var result = _blogPostService.GetPost(id.Value);
        if (result.IsSuccess)
        {
            var post = result.Value;
            CurrentModel = new EditModel
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
