using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Posts;

/// <summary>
///     Represents the page model for the admin posts page.
/// </summary>
public sealed class Index : PageModel
{
    private readonly BlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    public Index(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the list of blog posts.
    /// </summary>
    /// <value>The list of blog posts.</value>
    public IReadOnlyList<BlogPost> BlogPosts { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        BlogPosts = _blogPostService.GetAllBlogPosts(visibility: Visibility.None);
    }
}
