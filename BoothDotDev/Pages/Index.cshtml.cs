using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

internal sealed class Index : PageModel
{
    private const int RecentActivityCount = 5;
    private const int LatestPostCount = 3;
    private readonly ActivityService _activityService;
    private readonly BlogPostService _blogPostService;
    private readonly PortfolioService _portfolioService;
    private readonly StatusService _statusService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="activityService">The activity service.</param>
    /// <param name="portfolioService">The portfolio service.</param>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="statusService">The status service.</param>
    public Index(
        ActivityService activityService,
        PortfolioService portfolioService,
        BlogPostService blogPostService,
        StatusService statusService)
    {
        _activityService = activityService;
        _portfolioService = portfolioService;
        _blogPostService = blogPostService;
        _statusService = statusService;
    }

    /// <summary>
    ///     Gets the latest blog posts.
    /// </summary>
    /// <value>The latest blog posts.</value>
    public IReadOnlyList<BlogPost> LatestPosts { get; private set; } = [];

    /// <summary>
    ///     Gets the recent activity.
    /// </summary>
    /// <returns>The recent activity.</returns>
    public IReadOnlyList<ActivityEntry> RecentActivity { get; private set; } = [];

    /// <summary>
    ///     Gets the things I've made that are featured, in the order I chose.
    /// </summary>
    /// <returns>The featured items.</returns>
    public IReadOnlyList<PortfolioItem> Featured { get; private set; } = [];

    /// <summary>
    ///     Gets the status to show in the thought bubble.
    /// </summary>
    /// <value>The status, or <see langword="null" /> if none is in rotation.</value>
    public string? StatusText { get; private set; }

    /// <summary>
    ///     Handles the GET request for the index page.
    /// </summary>
    public void OnGet()
    {
        StatusText = _statusService.GetRandomActive();
        RecentActivity = _activityService.GetRecentActivity(new ActivitySearchOptions(RecentActivityCount));
        Featured = _portfolioService.GetFeatured();
        LatestPosts = _blogPostService.GetRecentBlogPosts(new ActivitySearchOptions(LatestPostCount));
    }
}
