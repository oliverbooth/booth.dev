using System.Drawing;

namespace BoothDotDev.Data;

/// <summary>
///     Represents a request to create or update an artwork item.
/// </summary>
/// <param name="Title">The title of the artwork.</param>
/// <param name="Description">The description of the artwork, if it has one.</param>
/// <param name="Published">The publication date and time of the artwork.</param>
/// <param name="Visibility">The visibility of the artwork.</param>
/// <param name="IsWorkInProgress">A value indicating whether the artwork is a work in progress.</param>
/// <param name="MadeWith">A string describing how the artwork was made, if specified.</param>
/// <param name="FileName">The bare filename of the artwork's uploaded file.</param>
/// <param name="Resolution">The pixel resolution of the artwork.</param>
public sealed record ArtworkItemSaveRequest(
    string Title,
    string? Description,
    DateTimeOffset PublishedAt,
    Visibility Visibility,
    bool IsWorkInProgress,
    string? MadeWith,
    string FileName,
    Size Resolution);
