using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Services;
using BC = BCrypt.Net.BCrypt;

namespace OliverBooth.Pages.Blog;

/// <summary>
///     Represents the page model for the <c>Article</c> page.
/// </summary>
[Area("blog")]
public class Article : PageModel
{
    private readonly IBlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Article" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="IBlogPostService" />.</param>
    public Article(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /*
    /// <summary>
    ///     Gets a value indicating whether the post is a legacy WordPress post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the post is a legacy WordPress post; otherwise, <see langword="false" />.
    /// </value>
    public bool IsWordPressLegacyPost => Post.WordPressId.HasValue;
    */

    /// <summary>
    ///     Gets the requested blog post.
    /// </summary>
    /// <value>The requested blog post.</value>
    public IBlogPost Post { get; private set; } = null!;

    /// <summary>
    ///     Gets a value indicating whether to show the password prompt.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the password prompt should be shown; otherwise, <see langword="false" />.
    /// </value>
    public bool ShowPasswordPrompt { get; private set; }

    public IActionResult OnGet(int year, int month, int day, string slug)
    {
        var date = new DateOnly(year, month, day);
        if (!_blogPostService.TryGetPost(date, slug, out IBlogPost? post))
        {
            Response.StatusCode = 404;
            return NotFound();
        }

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

    public IActionResult OnPost([FromRoute] int year,
        [FromRoute] int month,
        [FromRoute] int day,
        [FromRoute] string slug)
    {
        var date = new DateOnly(year, month, day);
        if (!_blogPostService.TryGetPost(date, slug, out IBlogPost? post))
        {
            return NotFound();
        }

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
