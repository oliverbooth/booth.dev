using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Projects;

internal sealed class Project : PageModel
{
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Project" /> class.
    /// </summary>
    /// <param name="projectService">The project service.</param>
    public Project(ProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    ///     Gets the selected project to display on the page.
    /// </summary>
    /// <value>The selected project.</value>
    public Data.Models.Project SelectedProject { get; private set; } = null!;

    /// <summary>
    ///     Handles the GET request for the project page.
    /// </summary>
    /// <param name="slug">The slug of the project to display.</param>
    /// <returns>The result of the GET request.</returns>
    public IActionResult OnGet(string slug)
    {
        if (!_projectService.TryGetProject(slug, out var project))
        {
            return NotFound();
        }

        SelectedProject = project;
        return Page();
    }
}
