using BoothDotDev.Data;
using BoothDotDev.Data.Models;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for fetching recent activity on the site, such as blog posts, devlog entries, and tutorial articles.
/// </summary>
public sealed class ActivityService
{
    private readonly BlogPostService _blogPostService;
    private readonly ProjectService _projectService;
    private readonly TutorialService _tutorialService;
    private readonly DevChallengeService _devChallengeService;
    private readonly NoteService _noteService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ActivityService" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="projectService">The project service.</param>
    /// <param name="tutorialService">The tutorial service.</param>
    /// <param name="devChallengeService">The dev challenge service.</param>
    /// <param name="noteService">The note service.</param>
    public ActivityService(BlogPostService blogPostService,
        ProjectService projectService,
        TutorialService tutorialService,
        DevChallengeService devChallengeService,
        NoteService noteService)
    {
        _blogPostService = blogPostService;
        _projectService = projectService;
        _tutorialService = tutorialService;
        _devChallengeService = devChallengeService;
        _noteService = noteService;
    }

    /// <summary>
    ///     Gets a read-only list of recent activity entries.
    /// </summary>
    /// <param name="count">The number of entries to return.</param>
    /// <param name="visibility">The visibility of the activity entries to return.</param>
    /// <returns>A read-only list of recent activity entries.</returns>
    public IReadOnlyList<ActivityEntry> GetRecentActivity(int count, Visibility visibility = Visibility.Published)
    {
        List<ActivityEntry> candidates =
        [
            .. _blogPostService.GetRecentBlogPosts(count, visibility).Select(ActivityEntryFactory.From),
            .. _tutorialService.GetRecentArticles(count, visibility).Select(a => ActivityEntryFactory.From(a, _tutorialService)),
            .. _projectService.GetRecentDevlogs(count, visibility).Select(p => ActivityEntryFactory.From(p,
                _projectService.TryGetProject(p.ProjectId, out var project)
                    ? project
                    : throw new InvalidOperationException($"Project with ID {p.ProjectId} not found."))),
            .. _devChallengeService.GetRecentChallenges(count, visibility).Select(ActivityEntryFactory.From),
            .. _noteService.GetRecentNotes(count, visibility).Select(ActivityEntryFactory.From)
        ];

        return candidates
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToList()
            .AsReadOnly();
    }
}
