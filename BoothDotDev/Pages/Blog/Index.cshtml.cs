using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

using Note = BoothDotDev.Data.Models.Note;

[Area("blog")]
internal sealed class Index : PageModel
{
    private readonly BlogPostService _blogPostService;
    private readonly NoteService _noteService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="noteService">The note service.</param>
    public Index(BlogPostService blogPostService, NoteService noteService)
    {
        _blogPostService = blogPostService;
        _noteService = noteService;
    }

    /// <summary>
    ///     Gets all blog posts.
    /// </summary>
    /// <value>All blog posts.</value>
    public IReadOnlyList<BlogPost> BlogPosts { get; private set; } = [];

    /// <summary>
    ///     Gets all blog post categories.
    /// </summary>
    /// <value>All blog post categories.</value>
    public IReadOnlyList<BlogPostCategory> BlogPostCategories { get; private set; } = [];

    /// <summary>
    ///     Gets all notes.
    /// </summary>
    /// <value>All notes.</value>
    public IReadOnlyList<Note> Notes { get; private set; } = [];

    /// <summary>
    ///     Gets the tag <see cref="BlogPosts" /> is currently filtered to, if any.
    /// </summary>
    /// <value>The active tag filter, or <see langword="null" /> if the page is showing everything.</value>
    public string? Tag { get; private set; }

    /// <summary>
    ///     Handles the GET request for the blog index page.
    /// </summary>
    /// <param name="postId">The post ID.</param>
    /// <param name="wpPostId">The WordPress post ID.</param>
    /// <param name="tag">
    ///     A tag to filter <see cref="BlogPosts" /> to. Ported from the 3.x site's <c>?tag=</c> query parameter, which
    ///     the redesign otherwise dropped - <c>Blog/Article.cshtml</c>'s tag links already point here.
    /// </param>
    /// <returns>The result of the GET request.</returns>
    public IActionResult OnGet([FromQuery(Name = "pid")] Guid? postId = null,
        [FromQuery(Name = "p")] int? wpPostId = null,
        [FromQuery(Name = "tag")] string? tag = null)
    {
        if (postId.HasValue != wpPostId.HasValue)
        {
            return postId.HasValue ? HandleNewRoute(postId.Value) : HandleWordPressRoute(wpPostId!.Value);
        }

        BlogPosts = _blogPostService.GetAllBlogPosts();
        BlogPostCategories = _blogPostService.GetTopLevelCategories();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            Tag = tag;
            ViewData["Title"] = $"Posts tagged “{tag.Replace('-', ' ')}”";
            BlogPosts = [.. BlogPosts.Where(p => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))];
        }
        else
        {
            Notes = _noteService.GetAllNotes();
        }

        return Page();
    }

    private IActionResult HandleNewRoute(Guid postId)
    {
        var result = _blogPostService.GetPost(postId);
        return result.IsSuccess ? RedirectToPost(result.Value) : NotFound();
    }

    private IActionResult HandleWordPressRoute(int wpPostId)
    {
        var result = _blogPostService.GetPost(wpPostId);
        return result.IsSuccess ? RedirectToPost(result.Value) : NotFound();
    }

    private RedirectResult RedirectToPost(BlogPost post)
    {
        var route = new
        {
            year = post.PublishedAt.ToString("yyyy"),
            month = post.PublishedAt.ToString("MM"),
            day = post.PublishedAt.ToString("dd"),
            slug = post.Slug
        };
        return Redirect(Url.Page("/Blog/Article", route)!);
    }
}
