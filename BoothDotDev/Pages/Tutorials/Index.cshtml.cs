using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Tutorials;

/// <summary>
///     Represents the index page for the tutorials.
/// </summary>
public sealed class Index : PageModel
{
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index"/> class.
    /// </summary>
    /// <param name="tutorialService">The tutorial service.</param>
    public Index(TutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    /// <summary>
    ///     Gets the current tutorial article.
    /// </summary>
    /// <value>The current tutorial article.</value>
    public TutorialArticle? CurrentArticle { get; private set; }

    /// <summary>
    ///     Gets the current tutorial folder.
    /// </summary>
    /// <value>The current tutorial folder.</value>
    public TutorialFolder? CurrentFolder { get; private set; }
    
    /// <summary>
    ///     Gets a value indicating whether to show the folder view.
    /// </summary>
    /// <value><see langword="true"/> if the folder view should be shown; otherwise, <see langword="false"/>.</value>
    public bool ShowFolderView { get; private set; }

    /// <summary>
    ///     Handles the GET request for the tutorial page based on the provided slug.
    /// </summary>
    /// <param name="slug">The slug of the tutorial page.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the request.</returns>
    public IActionResult OnGet([FromRoute(Name = "slug")] string? slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            CurrentFolder = null;
            ShowFolderView = true;
            return Page();
        }

        if (_tutorialService.TryGetFolder(slug, out var folder))
        {
            CurrentFolder = folder;
            ShowFolderView = true;
            return Page();
        }

        if (_tutorialService.TryGetArticle(slug, out var article))
        {
            CurrentArticle = article;
            ShowFolderView = false;
            return Page();
        }

        return NotFound();
    }
}
