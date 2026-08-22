namespace BoothDotDev.Data;

/// <summary>
///     Represents a single file or folder in a CDN directory listing.
/// </summary>
public sealed class CdnEntry
{
    /// <summary>
    ///     Gets the bare file or folder name.
    /// </summary>
    /// <value>The bare name.</value>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets a value indicating whether this entry is a folder rather than a file.
    /// </summary>
    /// <value><see langword="true" /> if this entry is a folder; otherwise, <see langword="false" />.</value>
    public required bool IsDirectory { get; init; }

    /// <summary>
    ///     Gets the fully-qualified CDN URL at which the file is served, or <see langword="null" /> for a folder.
    /// </summary>
    /// <value>The fully-qualified CDN URL, or <see langword="null" />.</value>
    public string? Url { get; init; }

    /// <summary>
    ///     Gets the media kind, as resolved from the file's extension, or <see langword="null" /> for a folder.
    /// </summary>
    /// <value>The media kind, or <see langword="null" />.</value>
    public MediaKind? Kind { get; init; }

    /// <summary>
    ///     Gets the size of the file, in bytes, or <see langword="null" /> for a folder.
    /// </summary>
    /// <value>The file size, in bytes, or <see langword="null" />.</value>
    public long? SizeBytes { get; init; }

    /// <summary>
    ///     Gets the number of immediate children of the folder, or <see langword="null" /> for a file.
    /// </summary>
    /// <value>The immediate child count, or <see langword="null" />.</value>
    public int? ItemCount { get; init; }

    /// <summary>
    ///     Gets the UTC timestamp the entry was last written.
    /// </summary>
    /// <value>The last-write timestamp, in UTC.</value>
    public required DateTimeOffset ModifiedAt { get; init; }
}
