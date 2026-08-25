namespace BoothDotDev.Data;

/// <summary>
///     Represents the options for the CDN.
/// </summary>
public sealed class CdnOptions
{
    /// <summary>
    ///     The name of the configuration section for CDN options.
    /// </summary>
    public const string SectionName = "CDN";

    /// <summary>
    ///     Gets or sets the base URL of the CDN.
    /// </summary>
    /// <value>The base URL of the CDN, e.g. <c>https://cdn.booth.dev</c>. No trailing slash.</value>
    public string BaseUrl { get; init; } = string.Empty;
}
