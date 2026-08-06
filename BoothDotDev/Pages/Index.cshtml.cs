using BoothDotDev.Common.Data.Models;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

public partial class Index : PageModel
{
    private readonly IBlogPostService _blogPostService;

    public Index(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the latest blog posts.
    /// </summary>
    /// <returns>The latest blog posts.</returns>
    public IReadOnlyList<IBlogPost> BlogPosts { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request for the index page.
    /// </summary>
    public void OnGet()
    {
        BlogPosts = _blogPostService.GetBlogPosts(0, 3);
    }
}
