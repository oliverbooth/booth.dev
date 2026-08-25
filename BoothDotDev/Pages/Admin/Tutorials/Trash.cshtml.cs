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

    /// <summary>
    ///     Handles the POST request for permanently deleting a single trashed article.
    /// </summary>
    /// <param name="id">The ID of the article to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDelete(Guid id)
    {
        _tutorialService.PermanentlyDeleteArticle(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting every selected trashed article.
    /// </summary>
    /// <param name="ids">The IDs of the articles to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDeleteBulk(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            _tutorialService.PermanentlyDeleteArticle(id);
        }

        return RedirectToPage();
    }
}
