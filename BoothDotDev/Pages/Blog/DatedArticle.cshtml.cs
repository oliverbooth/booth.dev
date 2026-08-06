using BoothDotDev.Common.Data.Models;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

/// <summary>
///     Represents the page model for the <c>Article</c> page.
/// </summary>
[Area("blog")]
internal sealed class DatedArticle : PageModel
{
    private readonly IBlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatedArticle" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="IBlogPostService" />.</param>
    public DatedArticle(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="year">The year of the article.</param>
    /// <param name="month">The month of the article.</param>
    /// <param name="day">The day of the article.</param>
    /// <param name="slug">The slug of the article.</param>
    /// <returns>The result of the GET request.</returns>
    public IActionResult OnGet(int year, int month, int day, string slug)
    {
        var date = new DateOnly(year, month, day);
        if (!_blogPostService.TryGetPost(date, slug, out IBlogPost? post))
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        return RedirectToPage("Article", new { slug = post.Slug });
    }
}