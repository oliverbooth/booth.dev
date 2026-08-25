using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Tutorials;

/// <summary>
///     Represents the page model for the admin tutorials page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    public Index(TutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    /// <summary>
    ///     Gets the list of articles.
    /// </summary>
    /// <value>The list of articles.</value>
    public IReadOnlyList<TutorialArticle> Articles { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Articles = _tutorialService.GetAllArticles();
    }

    /// <summary>
    ///     Handles the POST request for moving an article to the trash.
    /// </summary>
    /// <param name="id">The ID of the article to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        _tutorialService.TrashArticle(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Gets the full slug path of the specified article, for display in the listing.
    /// </summary>
    /// <param name="article">The article whose path to return.</param>
    /// <returns>The article's full slug path.</returns>
    public string GetPath(TutorialArticle article)
    {
        return _tutorialService.GetFullSlug(article);
    }
}
