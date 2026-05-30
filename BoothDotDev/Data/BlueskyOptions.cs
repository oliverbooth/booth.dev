namespace BoothDotDev.Data;

/// <summary>
///     Represents the options for Bluesky.
/// </summary>
public sealed class BlueskyOptions
{
    /// <summary>
    ///     The name of the configuration section for Bluesky options.
    /// </summary>
    public const string SectionName = "Bluesky";

    /// <summary>
    ///     Gets or sets the Bluesky handle.
    /// </summary>
    /// <value>The Bluesky handle.</value>
    public string Handle { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether to include reposts in the Bluesky feed.
    /// </summary>
    /// <value><see langword="true" /> if reposts should be included; otherwise, <see langword="false" />.</value>
    public bool IncludeReposts { get; init; } = false;

    /// <summary>
    ///     Gets or sets the duration in minutes for which to cache the Bluesky feed.
    /// </summary>
    /// <value>The duration in minutes for which to cache the Bluesky feed.</value>   
    public int CacheDurationMinutes { get; init; } = 5;
}
