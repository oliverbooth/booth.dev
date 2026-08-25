using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Projects;

/// <summary>
///     Represents the page model for the admin projects page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="projectService">The <see cref="ProjectService" />.</param>
    public Index(ProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    ///     Gets the list of projects.
    /// </summary>
    /// <value>The list of projects.</value>
    public IReadOnlyList<Project> Projects { get; private set; } = [];

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
        Projects = _projectService.GetAllProjects();
    }

    /// <summary>
    ///     Handles the POST request for deleting a project. The project must not have any devlog entries, trashed
    ///     or not.
    /// </summary>
    /// <param name="id">The ID of the project to delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        var result = _projectService.DeleteProject(id);
        if (result.IsFailed)
        {
            DeleteError = string.Join(Environment.NewLine, result.Errors.Select(e => e.Message));
        }

        return RedirectToPage();
    }
}
