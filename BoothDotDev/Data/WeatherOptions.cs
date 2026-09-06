namespace BoothDotDev.Data;

/// <summary>
///     Represents the options for the /now page's weather card.
/// </summary>
public sealed class WeatherOptions
{
    /// <summary>
    ///     The name of the configuration section for weather options.
    /// </summary>
    public const string SectionName = "Weather";

    /// <summary>
    ///     Gets or sets the latitude to report weather for.
    /// </summary>
    /// <value>The latitude.</value>
    public double Latitude { get; init; }

    /// <summary>
    ///     Gets or sets the longitude to report weather for.
    /// </summary>
    /// <value>The longitude.</value>
    public double Longitude { get; init; }

    /// <summary>
    ///     Gets or sets the number of minutes to cache the current weather for.
    /// </summary>
    /// <value>The cache duration, in minutes.</value>
    public int CacheDurationMinutes { get; init; } = 15;
}
