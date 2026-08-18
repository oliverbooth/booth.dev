using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

internal sealed class Index : PageModel
{
    private const int RecentActivityCount = 5;
    private const int ProjectCount = 6;
    private readonly ActivityService _activityService;
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="activityService">The activity service.</param>
    /// <param name="projectService">The project service.</param>
    public Index(ActivityService activityService, ProjectService projectService)
    {
        _activityService = activityService;
        _projectService = projectService;
    }

    /// <summary>
    ///     Gets the recent activity.
    /// </summary>
    /// <returns>The recent activity.</returns>
    public IReadOnlyList<ActivityEntry> RecentActivity { get; private set; } = [];

    /// <summary>
    ///     Gets the latest projects.
    /// </summary>
    /// <returns>The latest projects.</returns>
    public IReadOnlyList<Project> Projects { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request for the index page.
    /// </summary>
    public void OnGet()
    {
        RecentActivity = _activityService.GetRecentActivity(new ActivitySearchOptions(RecentActivityCount));
        Projects = [.. _projectService.GetProjects().Concat(_projectService.GetProjects(ProjectStatus.Past)).Take(ProjectCount)];
    }
}
