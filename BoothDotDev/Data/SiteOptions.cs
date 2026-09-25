namespace BoothDotDev.Data;

/// <summary>
///     Represents the options that describe this deployment of the site.
/// </summary>
public sealed class SiteOptions
{
    /// <summary>
    ///     The name of the configuration section for site options.
    /// </summary>
    public const string SectionName = "Site";

    /// <summary>
    ///     Gets or sets the label of a deployment that isn't the live site, such as <c>staging</c>. Setting it marks every page
    ///     with a banner and badge and asks search engines not to index the site.
    /// </summary>
    /// <value>The label, or <see langword="null" /> or empty on the live site.</value>
    public string? EnvironmentLabel { get; init; }
}
