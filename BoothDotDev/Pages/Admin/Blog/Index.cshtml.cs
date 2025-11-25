using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Blog;

/// <summary>
///     Represents a class which defines the model for the <c>/admin/blog</c> route.
/// </summary>
[Authorize]
public sealed class Index : PageModel
{
    private readonly IBlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    public Index(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the blog posts.
    /// </summary>
    /// <value>The blog posts.</value>
    public IReadOnlyList<IBlogPost> BlogPosts { get; private set; } = [];

    /// <summary>
    ///     Handles the incoming GET request to the page.
    /// </summary>
    public void OnGet()
    {
        BlogPosts = _blogPostService.GetEverySingleBlogPost();
    }
}
