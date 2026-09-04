using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace BoothDotDev.Vite;

internal sealed class ManifestEntry
{
    [JsonPropertyName("file")] public required string File { get; init; }

    [UsedImplicitly]
    [JsonPropertyName("src")]
    public string? Src { get; init; }

    [UsedImplicitly]
    [JsonPropertyName("isEntry")]
    public bool IsEntry { get; init; }

    [UsedImplicitly]
    [JsonPropertyName("css")]
    public string[]? Css { get; init; }

    [UsedImplicitly]
    [JsonPropertyName("imports")]
    public string[]? Imports { get; init; }
}
