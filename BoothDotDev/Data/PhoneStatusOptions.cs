namespace BoothDotDev.Data;

/// <summary>
///     Represents the options for the phone status hook on the /now page.
/// </summary>
public sealed class PhoneStatusOptions
{
    /// <summary>
    ///     The name of the configuration section for phone status options.
    /// </summary>
    public const string SectionName = "PhoneStatus";

    /// <summary>
    ///     Gets or sets the bearer secret the phone-side automation must present to report a status update.
    /// </summary>
    /// <value>The bearer secret.</value>
    public string Secret { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the number of minutes after which the last-reported status is considered stale and hidden.
    /// </summary>
    /// <value>The staleness threshold, in minutes.</value>
    public int StaleAfterMinutes { get; init; } = 180;
}
