using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

/// <summary>
///     Represents the page model for the <c>Article</c> page.
/// </summary>
[Area("blog")]
internal sealed class Article : PageModel
{
    private readonly BlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Article" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    public Article(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the requested blog post.
    /// </summary>
    /// <value>The requested blog post.</value>
    public BlogPost Post { get; private set; } = null!;

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="slug">The slug of the post to display.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(string slug)
    {
        var result = _blogPostService.GetPost(slug);
        if (result.IsFailed)
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        var post = result.Value;

        // Now that drafts exist, there's no legitimate reason for a signed-out visitor to reach a private post
        // by its public URL - the editor's preview pane covers previewing unpublished work.
        if (post.Visibility == Visibility.Private && User.Identity?.IsAuthenticated != true)
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        if (post.IsRedirect)
        {
            return Redirect(post.RedirectUrl!.ToString());
        }

        Post = post;
        return Page();
    }
}
