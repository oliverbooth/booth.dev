using Optional;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents an entry in the activity feed.
/// </summary>
public sealed record ActivityEntry
{
    /// <summary>
    ///     Gets or initializes the date and time when the activity entry was created.
    /// </summary>
    /// <value>The date and time when the activity entry was created.</value>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     Gets or initializes the title of the activity entry.
    /// </summary>
    /// <value>The title of the activity entry.</value>
    public required string Title { get; init; }

    /// <summary>
    ///     Gets or initializes the category of the activity entry.
    /// </summary>
    /// <value>The category of the activity entry.</value>
    public required string Category { get; init; }

    /// <summary>
    ///     Gets or initializes the path to the page for the activity entry.
    /// </summary>
    /// <value>The path to the page for the activity entry.</value>
    public required string PagePath { get; init; }

    /// <summary>
    ///     Gets or initializes the raw URL for the activity entry.
    /// </summary>
    /// <value>The raw URL for the activity entry.</value>
    public Option<string> RawUrl { get; init; }

    /// <summary>
    ///     Gets or initializes a dictionary of route values for the entry's page.
    /// </summary>
    /// <value>A dictionary of route values for the entry's page.</value>
    public Dictionary<string, string> RouteValues { get; init; } = [];

    /// <summary>
    ///     Gets or initializes the estimated reading time for this activity entry.
    /// </summary>
    /// <value>
    ///     An integer representing the estimated reading time in minutes, or <see langword="null" /> if the reading time is not
    ///     applicable.
    /// </value>
    public Option<int> ReadingMinutes { get; init; }

    /// <summary>
    ///     Gets or initializes the commit SHA string associated with the activity entry.
    /// </summary>
    /// <value>The commit SHA string associated with the activity entry.</value>
    /// <remarks>
    ///     This value is not necessarily an actual commit SHA, but may be a string that resembles a commit SHA. For this website,
    ///     this is typically the first 7 characters of the entity's UUID.
    /// </remarks>
    public required string CommitSha { get; init; } = string.Empty;
}
