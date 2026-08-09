using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

[Area("blog")]
internal sealed class Index : PageModel
{
    private readonly BlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index"/> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    public Index(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
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
        return Page();
    }

    private IActionResult HandleNewRoute(Guid postId)
    {
        return _blogPostService.TryGetPost(postId, out var post) ? RedirectToPost(post) : NotFound();
    }

    private IActionResult HandleWordPressRoute(int wpPostId)
    {
        return _blogPostService.TryGetPost(wpPostId, out var post) ? RedirectToPost(post) : NotFound();
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
