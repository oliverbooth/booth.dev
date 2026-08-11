namespace BoothDotDev.Views;

/// <summary>
///     Represents the model for a media link with alt text, title, and URL.
/// </summary>
public sealed class MediaLinkModel
{
    /// <summary>
    ///     Gets the alt text for the media link.
    /// </summary>
    /// <value>The alt text for the media link.</value>
    public required string Alt { get; init; }

    /// <summary>
    ///     Gets the MIME type of the media link.
    /// </summary>
    /// <value>The MIME type of the media link.</value>
    public required string? MimeType { get; init; }

    /// <summary>
    ///     Gets the title of the media link.
    /// </summary>
    /// <value>The title of the media link.</value>
    public string? Title { get; init; }

    /// <summary>
    ///     Gets the URL of the media link.
    /// </summary>
    /// <value>The URL of the media link.</value>
    public required string Url { get; init; }
}
