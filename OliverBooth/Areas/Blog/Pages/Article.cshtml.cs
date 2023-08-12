using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Areas.Blog.Pages;

/// <summary>
///     Represents the page model for the <c>Article</c> page.
/// </summary>
[Area("blog")]
public class Article : PageModel
{
    private readonly BlogService _blogService;
    private readonly BlogUserService _blogUserService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Article" /> class.
    /// </summary>
    /// <param name="blogService">The <see cref="BlogService" />.</param>
    /// <param name="blogUserService">The <see cref="BlogUserService" />.</param>
    public Article(BlogService blogService, BlogUserService blogUserService)
    {
        _blogService = blogService;
        _blogUserService = blogUserService;
    }

    /// <summary>
    ///     Gets the author of the post.
    /// </summary>
    /// <value>The author of the post.</value>
    public User Author { get; private set; } = null!;

    /// <summary>
    ///     Gets a value indicating whether the post is a legacy WordPress post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the post is a legacy WordPress post; otherwise, <see langword="false" />.
    /// </value>
    public bool IsWordPressLegacyPost => Post.WordPressId.HasValue;

    /// <summary>
    ///     Gets the requested blog post.
    /// </summary>
    /// <value>The requested blog post.</value>
    public BlogPost Post { get; private set; } = null!;

    public IActionResult OnGet(int year, int month, int day, string slug)
    {
        if (!_blogService.TryGetBlogPost(year, month, day, slug, out BlogPost? post))
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        Post = post;
        Author = _blogUserService.TryGetUser(post.AuthorId, out User? author) ? author : null!;
        return Page();
    }
}
