namespace BoothDotDev.Views;

/// <summary>
///     Represents a template for an image with alt text, title, and URL.
/// </summary>
public sealed class ImageTemplate
{
    /// <summary>
    ///     Gets the alt text for the image.
    /// </summary>
    /// <value>The alt text for the image.</value>
    public required string Alt { get; init; }

    /// <summary>
    ///     Gets the title of the image.
    /// </summary>
    /// <value>The title of the image.</value>
    public string? Title { get; init; }

    /// <summary>
    ///     Gets the URL of the image.
    /// </summary>
    /// <value>The URL of the image.</value>
    public required string Url { get; init; }
}
