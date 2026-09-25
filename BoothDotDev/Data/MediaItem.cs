namespace BoothDotDev.Data;

/// <summary>
///     Represents a file of a project or creation, ready to show: its details along with where it can be fetched from.
/// </summary>
/// <param name="Id">The ID of the file.</param>
/// <param name="FileName">The bare name of the file.</param>
/// <param name="Url">The URL of the file on the CDN.</param>
/// <param name="Kind">The kind of file it is.</param>
/// <param name="Alt">The alternative text describing the file, if it has any.</param>
/// <param name="IsCover">Whether the file is its owner's cover.</param>
/// <param name="Width">The width of the file in pixels, if it is an image.</param>
/// <param name="Height">The height of the file in pixels, if it is an image.</param>
/// <param name="Duration">The length of the file's audio, if it is audio.</param>
public sealed record MediaItem(
    Guid Id,
    string FileName,
    string Url,
    MediaKind Kind,
    string? Alt,
    bool IsCover,
    int? Width,
    int? Height,
    TimeSpan? Duration);
