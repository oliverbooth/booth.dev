using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Tutorials;

/// <summary>
///     Represents the page model for the <c>Article</c> page.
/// </summary>
public class Article : PageModel
{
    private readonly ITutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Article" /> class.
    /// </summary>
    /// <param name="tutorialService">The <see cref="ITutorialService" />.</param>
    public Article(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    /// <summary>
    ///     Gets the requested article.
    /// </summary>
    /// <value>The requested article.</value>
    public ITutorialArticle CurrentArticle { get; private set; } = null!;

    public IActionResult OnGet(string slug)
    {
        if (!_tutorialService.TryGetArticle(slug, out ITutorialArticle? article))
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        CurrentArticle = article;
        return Page();
    }
}
