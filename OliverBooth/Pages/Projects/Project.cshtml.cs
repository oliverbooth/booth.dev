using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Common.Data.Web;
using OliverBooth.Common.Services;

namespace OliverBooth.Pages.Projects;

public class Project : PageModel
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