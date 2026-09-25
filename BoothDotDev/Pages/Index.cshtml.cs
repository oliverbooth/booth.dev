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

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="activityService">The activity service.</param>
    /// <param name="portfolioService">The portfolio service.</param>
    /// <param name="blogPostService">The blog post service.</param>
    public Index(ActivityService activityService, PortfolioService portfolioService, BlogPostService blogPostService)
    {
        _activityService = activityService;
        _portfolioService = portfolioService;
        _blogPostService = blogPostService;
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
    ///     Handles the GET request for the index page.
    /// </summary>
    public void OnGet()
    {
        RecentActivity = _activityService.GetRecentActivity(new ActivitySearchOptions(RecentActivityCount));
        Featured = _portfolioService.GetFeatured();
        LatestPosts = _blogPostService.GetRecentBlogPosts(new ActivitySearchOptions(LatestPostCount));
    }
}
