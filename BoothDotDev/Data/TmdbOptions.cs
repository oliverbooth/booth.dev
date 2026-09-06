namespace BoothDotDev.Data;

/// <summary>
///     Represents the options for The Movie Database (TMDB) lookup.
/// </summary>
public sealed class TmdbOptions
{
    /// <summary>
    ///     The name of the configuration section for TMDB options.
    /// </summary>
    public const string SectionName = "Tmdb";

    /// <summary>
    ///     Gets or sets the TMDB API key.
    /// </summary>
    /// <value>The TMDB API key.</value>
    public string ApiKey { get; init; } = string.Empty;
}
