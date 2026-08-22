using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Tutorials.Folders;

/// <summary>
///     Represents the page model for the admin tutorial folders page.
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
    ///     Gets the list of folders.
    /// </summary>
    /// <value>The list of folders.</value>
    public IReadOnlyList<TutorialFolder> Folders { get; private set; } = [];

    /// <summary>
    ///     Gets the error message from a failed delete attempt, if any.
    /// </summary>
    /// <value>The error message, or <see langword="null" /> if the last delete attempt succeeded (or none was made).</value>
    [TempData]
    public string? DeleteError { get; set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Folders = _tutorialService.GetAllFolders();
    }

    /// <summary>
    ///     Handles the POST request for deleting a folder. The folder must not contain any child folders or
    ///     articles.
    /// </summary>
    /// <param name="id">The ID of the folder to delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        var result = _tutorialService.DeleteFolder(id);
        if (result.IsFailed)
        {
            DeleteError = string.Join(Environment.NewLine, result.Errors.Select(e => e.Message));
        }

        return RedirectToPage();
    }

    /// <summary>
    ///     Gets the full slug path of the specified folder, for display in the listing.
    /// </summary>
    /// <param name="folder">The folder whose path to return.</param>
    /// <returns>The folder's full slug path.</returns>
    public string GetPath(TutorialFolder folder)
    {
        return _tutorialService.GetFullSlug(folder);
    }
}
