using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Tutorials;

/// <summary>
///     Represents the page model for the admin tutorial trash page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Trash : PageModel
{
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    public Trash(TutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    /// <summary>
    ///     Gets the list of trashed articles, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed articles.</value>
    public IReadOnlyList<TutorialArticle> Articles { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Articles = _tutorialService.GetTrashedArticles();
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed article.
    /// </summary>
    /// <param name="id">The ID of the article to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        _tutorialService.RestoreArticle(id);
        return RedirectToPage();
    }
}
