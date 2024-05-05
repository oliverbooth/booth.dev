namespace OliverBooth.Common.Data.Mastodon;

public sealed class MediaAttachment
{
    /// <summary>
    ///     Gets the preview URL of the attachment.
    /// </summary>
    /// <value>The preview URL.</value>
    public Uri PreviewUrl { get; set; } = null!;

    /// <summary>
    ///     Gets the type of this attachment.
    /// </summary>
    /// <value>The attachment type.</value>
    public AttachmentType Type { get; set; } = AttachmentType.Unknown;

    /// <summary>
    ///     Gets the URL of the attachment.
    /// </summary>
    /// <value>The URL.</value>
    public Uri Url { get; set; } = null!;
}