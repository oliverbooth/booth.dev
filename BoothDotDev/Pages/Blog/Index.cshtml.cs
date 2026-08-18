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
    ///     Handles the GET request for the blog index page.
    /// </summary>
    /// <param name="postId">The post ID.</param>
    /// <param name="wpPostId">The WordPress post ID.</param>
    /// <returns>The result of the GET request.</returns>
    public IActionResult OnGet([FromQuery(Name = "pid")] Guid? postId = null,
        [FromQuery(Name = "p")] int? wpPostId = null)
    {
        if (postId.HasValue != wpPostId.HasValue)
        {
            return postId.HasValue ? HandleNewRoute(postId.Value) : HandleWordPressRoute(wpPostId!.Value);
        }

        BlogPosts = _blogPostService.GetAllBlogPosts();
        BlogPostCategories = _blogPostService.GetTopLevelCategories();
        Notes = _noteService.GetAllNotes();
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
            year = post.Published.ToString("yyyy"),
            month = post.Published.ToString("MM"),
            day = post.Published.ToString("dd"),
            slug = post.Slug
        };
        return Redirect(Url.Page("/Blog/Article", route)!);
    }
}
