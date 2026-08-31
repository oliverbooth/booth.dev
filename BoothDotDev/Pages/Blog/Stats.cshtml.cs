using BoothDotDev.Extensions;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Blog;

/// <summary>
///     Represents a class which defines the model for the <c>/blog/stats</c> route.
/// </summary>
internal sealed class Stats : PageModel
{
    private readonly BlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Stats" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    public Stats(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the average gap, in days, between consecutive posts.
    /// </summary>
    /// <value>The average gap, in days, between consecutive posts.</value>
    public double AverageGapDays { get; private set; }

    /// <summary>
    ///     Gets the average word count per post.
    /// </summary>
    /// <value>The average word count per post.</value>
    public double AverageWords { get; private set; }

    /// <summary>
    ///     Gets one calendar-heatmap grid per year that has at least one post, newest year first.
    /// </summary>
    /// <value>A list of <see cref="PostDistributionYear" /> instances, one for each year with at least one post.</value>
    public IReadOnlyList<PostDistributionYear> DistributionYears { get; private set; } = [];

    /// <summary>
    ///     Gets the date the first post was published.
    /// </summary>
    /// <value>The date the first post was published.</value>
    public DateOnly FirstPostDate { get; private set; }

    /// <summary>
    ///     Gets the date the most recent post was published.
    /// </summary>
    /// <value>The date the most recent post was published.</value>
    public DateOnly LastPostDate { get; private set; }

    /// <summary>
    ///     Gets the number of posts published in each year, newest year first.
    /// </summary>
    /// <value>A list of tuples, each containing a year and the number of posts published in that year.</value>
    public IReadOnlyList<(int Year, int Count)> PostsByYear { get; private set; } = [];

    /// <summary>
    ///     Gets the total number of fenced code blocks across every post.
    /// </summary>
    /// <value>The total number of fenced code blocks.</value>
    public int TotalCodeSamples { get; private set; }

    /// <summary>
    ///     Gets the total number of published posts.
    /// </summary>
    /// <value>The total number of published posts.</value>
    public int TotalPosts { get; private set; }

    /// <summary>
    ///     Gets the total word count across every post.
    /// </summary>
    /// <value>The total word count.</value>
    public int TotalWords { get; private set; }

    /// <summary>
    ///     Handles the incoming GET request to the page.
    /// </summary>
    public void OnGet()
    {
        var posts = _blogPostService.GetAllBlogPosts();
        TotalPosts = posts.Count;

        if (TotalPosts == 0)
        {
            return;
        }

        TotalWords = posts.Sum(post => post.GetWordCount());
        TotalCodeSamples = posts.Sum(post => post.GetCodeSampleCount());
        AverageWords = (double)TotalWords / TotalPosts;

        var publishDates = new HashSet<DateOnly>(posts.Select(post => DateOnly.FromDateTime(post.PublishedAt.DateTime)));
        FirstPostDate = publishDates.Min();
        LastPostDate = publishDates.Max();

        // sum of gaps between consecutive published dates telescopes to (last - first), regardless of what falls
        // between them, so this is equivalent to averaging every individual post-to-post gap
        AverageGapDays = TotalPosts > 1
            ? (LastPostDate.DayNumber - FirstPostDate.DayNumber) / (double)(TotalPosts - 1)
            : 0;

        PostsByYear = [.. posts
            .GroupBy(post => post.PublishedAt.Year)
            .OrderByDescending(group => group.Key)
            .Select(group => (Year: group.Key, Count: group.Count()))];

        DistributionYears = [.. Enumerable.Range(FirstPostDate.Year, LastPostDate.Year - FirstPostDate.Year + 1)
            .OrderByDescending(year => year)
            .Select(year => BuildDistributionYear(year, publishDates))];
    }

    /// <summary>
    ///     Builds one year's calendar-heatmap grid: one column per week (Sunday-start), one row per day of week -
    ///     the same layout GitHub's contribution graph uses.
    /// </summary>
    /// <param name="year">The calendar year to build a grid for.</param>
    /// <param name="publishDates">Every date a post was published on, across all years.</param>
    private static PostDistributionYear BuildDistributionYear(int year, IReadOnlySet<DateOnly> publishDates)
    {
        var jan1 = new DateOnly(year, 1, 1);
        var dec31 = new DateOnly(year, 12, 31);
        DateOnly gridStart = jan1.AddDays(-(int)jan1.DayOfWeek); // preceding Sunday, or Jan 1 itself if already one
        var columns = (dec31.DayNumber - gridStart.DayNumber + 7) / 7;

        var days = new List<PostDistributionDay>(columns * 7);
        for (var column = 0; column < columns; column++)
        {
            for (var row = 0; row < 7; row++)
            {
                DateOnly date = gridStart.AddDays(column * 7 + row);
                var inYear = date.Year == year;
                days.Add(new PostDistributionDay(inYear ? date : null, publishDates.Count(d => d == date)));
            }
        }

        var months = new List<PostDistributionMonth>(12);
        for (var month = 1; month <= 12; month++)
        {
            var firstOfMonth = new DateOnly(year, month, 1);
            var column = (firstOfMonth.DayNumber - gridStart.DayNumber) / 7;
            months.Add(new PostDistributionMonth(firstOfMonth.ToString("MMM"), column));
        }

        return new PostDistributionYear(year, columns, months, days);
    }
}

/// <summary>
///     Represents one year's worth of post-publication activity, laid out as a GitHub-style calendar grid.
/// </summary>
/// <param name="Year">The calendar year.</param>
/// <param name="ColumnCount">The number of week-columns in the grid.</param>
/// <param name="Months">The month labels and the column each one starts at.</param>
/// <param name="Days">Every cell in the grid, in column-major order (top-to-bottom within each week, left-to-right across weeks).</param>
internal sealed record PostDistributionYear(int Year, int ColumnCount, IReadOnlyList<PostDistributionMonth> Months, IReadOnlyList<PostDistributionDay> Days);

/// <summary>
///     Represents a month label positioned above a <see cref="PostDistributionYear" />'s grid.
/// </summary>
/// <param name="Name">The abbreviated month name, e.g. <c>"Jan"</c>.</param>
/// <param name="Column">The zero-based grid column the month's first week starts at.</param>
internal readonly record struct PostDistributionMonth(string Name, int Column);

/// <summary>
///     Represents a single day cell in a <see cref="PostDistributionYear" />'s grid.
/// </summary>
/// <param name="Date">The calendar date this cell represents, or <see langword="null" /> if it falls outside the year.</param>
/// <param name="PostCount">The number of posts published on this date.</param>
internal readonly record struct PostDistributionDay(DateOnly? Date, int PostCount);
