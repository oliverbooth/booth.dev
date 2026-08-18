namespace BoothDotDev.Data;

/// <summary>
///     Represents the strategy for sorting activity entries in the activity feed.
/// </summary>
public enum ActivitySortStrategy
{
    /// <summary>
    ///     Sorts activity entries by their creation date, with the most recently published entries appearing first.
    /// </summary>
    Published,

    /// <summary>
    ///     Sorts activity entries by their last updated date, with the most recently updated entries appearing first.
    /// </summary>
    Updated
}

/// <summary>
///     Represents the options for searching and retrieving activity entries from the activity feed.
/// </summary>
/// <param name="Count">The number of activity entries to retrieve.</param>
/// <param name="Visibility">The visibility level of the activity entries to retrieve.</param>
/// <param name="SortStrategy">The strategy for sorting the retrieved activity entries.</param>
public readonly record struct ActivitySearchOptions(
    int Count,
    Visibility Visibility = Visibility.Published,
    ActivitySortStrategy SortStrategy = ActivitySortStrategy.Published);
