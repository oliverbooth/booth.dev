using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Projects;

internal sealed class Devlog : PageModel
{
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Devlog" /> class.
    /// </summary>
    /// <param name="projectService">The project service.</param>
    public Devlog(ProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    ///     Gets the selected devlog entry.
    /// </summary>
    /// <value>The selected devlog entry.</value>
    public ProjectDevlog SelectedDevlog { get; private set; } = null!;

    /// <summary>
    ///     Gets the project associated with the selected devlog entry.
    /// </summary>
    /// <value>The project associated with the selected devlog entry.</value>
    public Data.Models.Project Project { get; private set; } = null!;

    /// <summary>
    ///     Handles GET requests for the DevLog page.
    /// </summary>
    /// <param name="projectSlug">The slug of the project associated with the devlog.</param>
    /// <param name="slug">The slug of the devlog entry.</param>
    /// <returns>An IActionResult representing the result of the GET request.</returns>
    public IActionResult OnGet(string projectSlug, string slug)
    {
        if (!_projectService.TryGetProject(projectSlug, out var project))
        {
            return NotFound();
        }

        Project = project;

        if (!_projectService.TryGetDevlog(project, slug, out var devlog))
        {
            return NotFound();
        }

        SelectedDevlog = devlog;
        return Page();
    }
}
