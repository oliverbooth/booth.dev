using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

/// <summary>
///     Represents the page model for the <c>RawArticle</c> page.
/// </summary>
[Area("blog")]
internal sealed class DatedRawArticle : PageModel
{
    private readonly BlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatedRawArticle" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    public DatedRawArticle(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    public IActionResult OnGet(int year, int month, int day, string slug)
    {
        var date = new DateOnly(year, month, day);
        if (!_blogPostService.TryGetPost(date, slug, out var post))
        {
            return NotFound();
        }

        return RedirectToPage("RawArticle", new { slug = post.Slug });
    }
}
