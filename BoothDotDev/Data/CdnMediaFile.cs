namespace BoothDotDev.Data;

/// <summary>
///     Represents a single file in a post's CDN media folder.
/// </summary>
public sealed class CdnMediaFile
{
    /// <summary>
    ///     Gets the bare filename, as referenced from Markdown (e.g. <c>![alt](filename.png)</c>).
    /// </summary>
    /// <value>The bare filename.</value>
    public required string FileName { get; init; }

    /// <summary>
    ///     Gets the fully-qualified CDN URL at which the file is served.
    /// </summary>
    /// <value>The fully-qualified CDN URL.</value>
    public required string Url { get; init; }

    /// <summary>
    ///     Gets the media kind, as resolved from the file's extension.
    /// </summary>
    /// <value>The media kind.</value>
    public required MediaKind Kind { get; init; }

    /// <summary>
    ///     Gets the size of the file, in bytes.
    /// </summary>
    /// <value>The file size, in bytes.</value>
    public required long SizeBytes { get; init; }

    /// <summary>
    ///     Gets the UTC timestamp the file was last written.
    /// </summary>
    /// <value>The last-write timestamp, in UTC.</value>
    public required DateTimeOffset ModifiedAt { get; init; }
}
