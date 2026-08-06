using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Tutorials;

/// <summary>
///     Represents the page model for the <c>Article</c> page.
/// </summary>
internal sealed class Article : PageModel
{
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Article" /> class.
    /// </summary>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    public Article(TutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    /// <summary>
    ///     Gets the requested article.
    /// </summary>
    /// <value>The requested article.</value>
    public TutorialArticle CurrentArticle { get; private set; } = null!;

    public IActionResult OnGet(string slug)
    {
        if (!_tutorialService.TryGetArticle(slug, out TutorialArticle? article))
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        CurrentArticle = article;
        return Page();
    }
}
