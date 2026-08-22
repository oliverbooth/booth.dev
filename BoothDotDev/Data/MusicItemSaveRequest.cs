namespace BoothDotDev.Data;

/// <summary>
///     Represents a request to create or update a music item.
/// </summary>
/// <param name="Title">The title of the track.</param>
/// <param name="Description">The description of the track, if it has one.</param>
/// <param name="Published">The publication date and time of the track.</param>
/// <param name="Visibility">The visibility of the track.</param>
/// <param name="IsWorkInProgress">A value indicating whether the track is a work in progress.</param>
/// <param name="MadeWith">A string describing how the track was made, if specified.</param>
/// <param name="FileName">The bare filename of the track's uploaded file.</param>
/// <param name="Duration">The duration of the track.</param>
public sealed record MusicItemSaveRequest(
    string Title,
    string? Description,
    DateTimeOffset Published,
    Visibility Visibility,
    bool IsWorkInProgress,
    string? MadeWith,
    string FileName,
    TimeSpan Duration);
