using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Projects;

internal sealed class Project : PageModel
{
    private readonly IProjectService _projectService;

    public Project(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public IProject? SelectedProject { get; private set; }

    public void OnGet(string slug)
    {
        if (_projectService.TryGetProject(slug, out IProject? project))
        {
            SelectedProject = project;
        }
    }
}
