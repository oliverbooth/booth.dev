using System.Text.Json.Serialization;

namespace OliverBooth.Data;

internal sealed class MastodonStatus : IMastodonStatus
{
    /// <inheritdoc />
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <inheritdoc />
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("url")]
    public Uri OriginalUri { get; set; } = null!;
}
