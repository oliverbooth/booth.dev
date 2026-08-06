using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Projects;

internal sealed class Project : PageModel
{
    private readonly ProjectService _projectService;

    public Project(ProjectService projectService)
    {
        _projectService = projectService;
    }

    public Data.Models.Project? SelectedProject { get; private set; }

    public void OnGet(string slug)
    {
        if (_projectService.TryGetProject(slug, out var project))
        {
            SelectedProject = project;
        }
    }
}
