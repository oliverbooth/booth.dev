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
    /// <param name="searchOptions">The options for searching and retrieving activity entries.</param>
    /// <returns>A read-only list of recent activity entries.</returns>
    public IReadOnlyList<ActivityEntry> GetRecentActivity(ActivitySearchOptions searchOptions)
    {
        List<ActivityEntry> candidates =
        [
            .. _blogPostService.GetRecentBlogPosts(searchOptions)
                .Select(ActivityEntryFactory.From),
            .. _tutorialService.GetRecentArticles(searchOptions)
                .Select(a => ActivityEntryFactory.From(a, _tutorialService)),
            .. _projectService.GetRecentDevlogs(searchOptions)
                .Select(p => (Devlog: p, Project: _projectService.TryGetProject(p.ProjectId, out var project) ? project : null))
                .Where(x => x.Project is not null)
                .Select(x => ActivityEntryFactory.From(x.Devlog, x.Project!)),
            .. _devChallengeService.GetRecentChallenges(searchOptions)
                .Select(ActivityEntryFactory.From),
            .. _noteService.GetRecentNotes(searchOptions)
                .Select(ActivityEntryFactory.From)
        ];

        var ordered = searchOptions.SortStrategy switch
        {
            ActivitySortStrategy.Published => candidates.OrderByDescending(e => e.PublishedAt),
            ActivitySortStrategy.Updated => candidates.OrderByDescending(e => e.UpdatedAt ?? e.PublishedAt),
            _ => throw new ArgumentOutOfRangeException(nameof(searchOptions.SortStrategy), searchOptions.SortStrategy, null)
        };

        return [.. ordered.Take(searchOptions.Count)];
    }
}
