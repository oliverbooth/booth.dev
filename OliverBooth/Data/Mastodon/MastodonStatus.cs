using System.Text.Json.Serialization;
using OliverBooth.Common.Data.Mastodon;

namespace OliverBooth.Data.Mastodon;

/// <inheritdoc />
internal sealed class MastodonStatus : IMastodonStatus
{
    /// <inheritdoc />
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("media_attachments")]
    public IReadOnlyList<MediaAttachment> MediaAttachments { get; set; } = ArraySegment<MediaAttachment>.Empty;

    /// <inheritdoc />
    [JsonPropertyName("url")]
    public Uri OriginalUri { get; set; } = null!;
}
