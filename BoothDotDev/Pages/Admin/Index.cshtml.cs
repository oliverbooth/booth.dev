using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin;

/// <summary>
///     Represents the dashboard for the admin section.
/// </summary>
[Authorize("Admin")]
public sealed class Index : PageModel
{
    private const int RecentActivityCount = 5;
    private readonly ActivityService _activityService;
    private readonly BlogPostService _blogPostService;
    private readonly NoteService _noteService;
    private readonly ProjectService _projectService;
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="activityService">The activity service.</param>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="noteService">The note service.</param>
    /// <param name="projectService">The project service.</param>
    /// <param name="tutorialService">The tutorial service.</param>
    public Index(ActivityService activityService,
        BlogPostService blogPostService,
        NoteService noteService,
        ProjectService projectService,
        TutorialService tutorialService)
    {
        _activityService = activityService;
        _blogPostService = blogPostService;
        _noteService = noteService;
        _projectService = projectService;
        _tutorialService = tutorialService;
    }

    /// <summary>
    ///     Gets the total number of blog posts.
    /// </summary>
    /// <value>The total number of blog posts.</value>
    public int BlogPostCount { get; private set; }

    /// <summary>
    ///     Gets the total number of notes.
    /// </summary>
    /// <value>The total number of notes.</value>
    public int NoteCount { get; private set; }

    /// <summary>
    ///     Gets the total number of projects.
    /// </summary>
    /// <value>The total number of projects.</value>
    public int ProjectCount { get; private set; }

    /// <summary>
    ///     Gets a read-only view of recent activity entries, including blog posts, devlog entries, and tutorial articles.
    /// </summary>
    /// <value>A read-only view of recent activity entries.</value>
    public IReadOnlyList<ActivityEntry> RecentActivity { get; private set; } = [];

    /// <summary>
    ///     Gets the total number of tutorials.
    /// </summary>
    /// <value>The total number of tutorials.</value>
    public int TutorialCount { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        BlogPostCount = _blogPostService.GetBlogPostCount();
        NoteCount = _noteService.GetNoteCount();
        ProjectCount = _projectService.GetProjectCount();
        TutorialCount = _tutorialService.GetArticleCount();

        RecentActivity = _activityService.GetRecentActivity(RecentActivityCount, Visibility.None);
    }
}
