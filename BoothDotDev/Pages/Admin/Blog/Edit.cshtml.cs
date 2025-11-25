using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Blog;

/// <summary>
///     Represents a class which defines the model for the <c>/admin/blog/edit</c> route.
/// </summary>
[Authorize]
public sealed class Edit : PageModel
{
    private readonly IBlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    public Edit(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the blog post.
    /// </summary>
    /// <value>The blog post.</value>
    public IBlogPost BlogPost { get; private set; } = null!;

    /// <summary>
    ///     Handles the incoming GET request to the page.
    /// </summary>
    /// <param name="id">The blog post ID.</param>
    public void OnGet([FromRoute(Name = "id")] Guid id)
    {
        if (id == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "A valid post ID must be specified");
            return;
        }

        if (!_blogPostService.TryGetPost(id, out IBlogPost? post))
        {
            ModelState.AddModelError(string.Empty, $"Post <code>{id}</code> not found");
            return;
        }

        BlogPost = post;
    }
}
