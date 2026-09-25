using System.Drawing;

namespace BoothDotDev.Data;

/// <summary>
///     Represents a request to create or update a creation.
/// </summary>
/// <param name="Kind">The kind of creation.</param>
/// <param name="Title">The title of the creation.</param>
/// <param name="Description">The description of the creation, if it has one.</param>
/// <param name="PublishedAt">The publication date and time of the creation.</param>
/// <param name="Visibility">The visibility of the creation.</param>
/// <param name="IsWorkInProgress">A value indicating whether the creation is a work in progress.</param>
/// <param name="MadeWith">A string describing how the creation was made, if specified.</param>
/// <param name="FileName">The bare filename of the creation's uploaded file.</param>
/// <param name="Resolution">The pixel resolution of the creation's image, or <see langword="null" /> if it has none.</param>
/// <param name="Duration">The duration of the creation's audio, or <see langword="null" /> if it has none.</param>
public sealed record CreationSaveRequest(
    CreationKind Kind,
    string Title,
    string? Description,
    DateTimeOffset PublishedAt,
    Visibility Visibility,
    bool IsWorkInProgress,
    string? MadeWith,
    string FileName,
    Size? Resolution = null,
    TimeSpan? Duration = null);
