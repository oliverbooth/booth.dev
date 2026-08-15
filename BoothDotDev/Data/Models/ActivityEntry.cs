namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents an entry in the activity feed.
/// </summary>
/// <param name="CreatedAt">The date and time when the activity entry was created.</param>
/// <param name="Title">The title of the activity entry.</param>
/// <param name="Category">The category of the activity entry.</param>
/// <param name="PagePath">The path to the page for the activity entry.</param>
/// <param name="RouteValues">The route values for the activity entry.</param>
/// <param name="CommitSha">The commit SHA string associated with the activity entry.</param>
public abstract record ActivityEntry(
    DateTimeOffset CreatedAt,
    string Title,
    string Category,
    string PagePath,
    Dictionary<string, string> RouteValues,
    int? ReadingMinutes,
    string CommitSha)
{
    /// <summary>
    ///     Represents a blog post activity entry.
    /// </summary>
    /// <param name="CreatedAt">The date and time when the activity entry was created.</param>
    /// <param name="Title">The title of the activity entry.</param>
    /// <param name="Category">The category of the activity entry.</param>
    /// <param name="RouteValues">The route values for the activity entry.</param>
    /// <param name="ReadingMinutes">The number of minutes it took to read the activity entry.</param>
    /// <param name="CommitSha">The commit SHA string associated with the activity entry.</param>
    public sealed record Blog(
        DateTimeOffset CreatedAt,
        string Title,
        string Category,
        Dictionary<string, string> RouteValues,
        int? ReadingMinutes,
        string CommitSha)
        : ActivityEntry(CreatedAt, Title, Category, "/Blog/Article", RouteValues, ReadingMinutes, CommitSha);

    /// <summary>
    ///     Represents a tutorial article activity entry.
    /// </summary>
    /// <param name="CreatedAt">The date and time when the activity entry was created.</param>
    /// <param name="Title">The title of the activity entry.</param>
    /// <param name="Category">The category of the activity entry.</param>
    /// <param name="RouteValues">The route values for the activity entry.</param>
    /// <param name="ReadingMinutes">The number of minutes it took to read the activity entry.</param>
    /// <param name="CommitSha">The commit SHA string associated with the activity entry.</param>
    public sealed record Tutorial(
        DateTimeOffset CreatedAt,
        string Title,
        string Category,
        Dictionary<string, string> RouteValues,
        int? ReadingMinutes,
        string CommitSha)
        : ActivityEntry(CreatedAt, Title, Category, "/Learn/Tutorials/Index", RouteValues, ReadingMinutes, CommitSha);

    /// <summary>
    ///     Represents a devlog activity entry.
    /// </summary>
    /// <param name="CreatedAt">The date and time when the activity entry was created.</param>
    /// <param name="Title">The title of the activity entry.</param>
    /// <param name="Category">The category of the activity entry.</param>
    /// <param name="RouteValues">The route values for the activity entry.</param>
    /// <param name="ReadingMinutes">The number of minutes it took to read the activity entry.</param>
    /// <param name="CommitSha">The commit SHA string associated with the activity entry.</param>
    public sealed record Devlog(
        DateTimeOffset CreatedAt,
        string Title,
        string Category,
        Dictionary<string, string> RouteValues,
        int? ReadingMinutes,
        string CommitSha)
        : ActivityEntry(CreatedAt, Title, Category, "/Projects/Devlog", RouteValues, ReadingMinutes, CommitSha);

    /// <summary>
    ///     Represents a challenge activity entry.
    /// </summary>
    /// <param name="CreatedAt">The date and time when the activity entry was created.</param>
    /// <param name="Title">The title of the activity entry.</param>
    /// <param name="Category">The category of the activity entry.</param>
    /// <param name="RouteValues">The route values for the activity entry.</param>
    /// <param name="CommitSha">The commit SHA string associated with the activity entry.</param>
    public sealed record Challenge(
        DateTimeOffset CreatedAt,
        string Title,
        string Category,
        Dictionary<string, string> RouteValues,
        string CommitSha)
        : ActivityEntry(CreatedAt, Title, Category, "/Learn/Challenges/Challenge", RouteValues, null, CommitSha);
}
