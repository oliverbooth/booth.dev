using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using BC = BCrypt.Net.BCrypt;

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
    ///     Gets a value indicating whether to show the password prompt.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the password prompt should be shown; otherwise, <see langword="false" />.
    /// </value>
    public bool ShowPasswordPrompt { get; private set; }

    public IActionResult OnGet(string slug)
    {
        var result = _blogPostService.GetPost(slug);
        if (!result.IsSuccessful)
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        var post = result.Value;
        if (!string.IsNullOrWhiteSpace(post.Password))
        {
            ShowPasswordPrompt = true;
        }

        if (post.IsRedirect)
        {
            return Redirect(post.RedirectUrl!.ToString());
        }

        Post = post;
        return Page();
    }

    public IActionResult OnPost([FromRoute] string slug)
    {
        var result = _blogPostService.GetPost(slug);
        if (!result.IsSuccessful)
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        var post = result.Value;
        ShowPasswordPrompt = true;

        if (Request.Form.TryGetValue("password", out StringValues password) && BC.Verify(password, post.Password))
        {
            ShowPasswordPrompt = false;
        }

        if (post.IsRedirect)
        {
            return Redirect(post.RedirectUrl!.ToString());
        }

        Post = post;
        return Page();
    }
}
