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
    ///     Gets the articles, grouped by their containing folder, in folder rank order.
    /// </summary>
    /// <value>The folder groups.</value>
    public IReadOnlyList<FolderGroup> FolderGroups { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        var folders = _tutorialService.GetAllFolders().ToDictionary(f => f.Id);
        var articles = _tutorialService.GetAllArticles();

        FolderGroups = articles
            .GroupBy(a => a.Folder)
            .Select(g => new FolderGroup(
                folders.GetValueOrDefault(g.Key),
                g.OrderBy(a => a.Rank).ToArray()))
            .OrderBy(g => g.Folder?.Rank ?? int.MaxValue)
            .ThenBy(g => g.Folder?.Title)
            .ToArray();
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

    /// <summary>
    ///     Represents a folder and the articles it directly contains, for display in the admin listing.
    /// </summary>
    /// <param name="Folder">The folder, or <see langword="null" /> if the articles' folder no longer exists.</param>
    /// <param name="Articles">The articles, in rank order.</param>
    public sealed record FolderGroup(TutorialFolder? Folder, IReadOnlyList<TutorialArticle> Articles);
}
