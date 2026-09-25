namespace BoothDotDev.Data;

/// <summary>
///     Represents a request to create or update a tutorial folder.
/// </summary>
/// <param name="Title">The title of the folder.</param>
/// <param name="Slug">The slug of the folder.</param>
/// <param name="Description">The description of the folder, if it has one.</param>
/// <param name="PreviewImageUrl">The URL of the folder's preview image, if it has one.</param>
/// <param name="Visibility">The visibility of the folder.</param>
/// <param name="Rank">The rank of the folder within its parent.</param>
/// <param name="Parent">The ID of the folder's parent, or <see langword="null" /> if the folder is at the root.</param>
/// <param name="Color">The colour of the folder, or <see langword="null" /> to derive it from the folder's ID.</param>
public sealed record TutorialFolderSaveRequest(
    string Title,
    string Slug,
    string? Description,
    Uri? PreviewImageUrl,
    Visibility Visibility,
    int Rank,
    Guid? Parent,
    PaletteHue? Color);
