using System.Text.Json.Serialization;

namespace OliverBooth.Data.Mastodon;

public sealed class MastodonStatus
{
    /// <summary>
    ///     Gets the content of the status.
    /// </summary>
    /// <value>The content.</value>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the date and time at which this status was posted.
    /// </summary>
    /// <value>The post timestamp.</value>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    ///     Gets the media attachments for this status.
    /// </summary>
    /// <value>The media attachments.</value>
    [JsonPropertyName("media_attachments")]
    public IReadOnlyList<MediaAttachment> MediaAttachments { get; set; } = ArraySegment<MediaAttachment>.Empty; 

    /// <summary>
    ///     Gets the original URI of the status.
    /// </summary>
    /// <value>The original URI.</value>
    [JsonPropertyName("url")]
    public Uri OriginalUri { get; set; } = null!;
}
