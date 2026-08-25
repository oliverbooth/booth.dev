using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Projects.Devlogs;

/// <summary>
///     Represents the page model for the admin project devlog trash page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Trash : PageModel
{
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="projectService">The <see cref="ProjectService" />.</param>
    public Trash(ProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    ///     Gets the project whose trashed devlogs are being viewed.
    /// </summary>
    /// <value>The project.</value>
    public Project Project { get; private set; } = null!;

    /// <summary>
    ///     Gets the list of the project's trashed devlogs, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed devlogs.</value>
    public IReadOnlyList<ProjectDevlog> Devlogs { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="projectId">The ID of the project whose trashed devlogs to view.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid projectId)
    {
        var projectResult = _projectService.GetProject(projectId);
        if (projectResult.IsFailed)
        {
            return NotFound();
        }

        Project = projectResult.Value;
        Devlogs = _projectService.GetTrashedDevlogs(Project);
        return Page();
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed devlog entry.
    /// </summary>
    /// <param name="projectId">The ID of the project the devlog entry belongs to.</param>
    /// <param name="id">The ID of the devlog entry to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid projectId, Guid id)
    {
        _projectService.RestoreDevlog(id);
        return RedirectToPage(new { projectId });
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting a single trashed devlog entry.
    /// </summary>
    /// <param name="projectId">The ID of the project the devlog entry belongs to.</param>
    /// <param name="id">The ID of the devlog entry to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDelete(Guid projectId, Guid id)
    {
        _projectService.PermanentlyDeleteDevlog(id);
        return RedirectToPage(new { projectId });
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting every selected trashed devlog entry.
    /// </summary>
    /// <param name="projectId">The ID of the project the devlog entries belong to.</param>
    /// <param name="ids">The IDs of the devlog entries to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDeleteBulk(Guid projectId, List<Guid> ids)
    {
        foreach (var id in ids)
        {
            _projectService.PermanentlyDeleteDevlog(id);
        }

        return RedirectToPage(new { projectId });
    }
}
