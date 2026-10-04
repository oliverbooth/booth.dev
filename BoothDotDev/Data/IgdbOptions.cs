namespace BoothDotDev.Data;

/// <summary>
///     Represents the options for the Internet Game Database (IGDB) lookup.
/// </summary>
public sealed class IgdbOptions
{
    /// <summary>
    ///     The name of the configuration section for IGDB options.
    /// </summary>
    public const string SectionName = "Igdb";

    /// <summary>
    ///     Gets or sets the Twitch application client ID.
    /// </summary>
    /// <value>The Twitch application client ID.</value>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the Twitch application client secret.
    /// </summary>
    /// <value>The Twitch application client secret.</value>
    public string ClientSecret { get; init; } = string.Empty;
}
