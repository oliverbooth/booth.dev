namespace BoothDotDev.Data;

/// <summary>
///     Represents the options for the Trakt integration.
/// </summary>
public sealed class TraktOptions
{
    /// <summary>
    ///     The name of the configuration section for Trakt options.
    /// </summary>
    public const string SectionName = "Trakt";

    /// <summary>
    ///     Gets or sets the Trakt application's client ID.
    /// </summary>
    /// <value>The client ID.</value>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the Trakt application's client secret.
    /// </summary>
    /// <value>The client secret.</value>
    public string ClientSecret { get; init; } = string.Empty;
}
