namespace BoothDotDev.Data;

/// <summary>
///     Represents a request to create or update a creation.
/// </summary>
/// <param name="Kind">The kind of creation.</param>
/// <param name="Title">The title of the creation.</param>
/// <param name="Slug">The slug of the creation. If blank, one is made from the title.</param>
/// <param name="Description">The description of the creation, if it has one.</param>
/// <param name="PublishedAt">The publication date and time of the creation.</param>
/// <param name="Visibility">The visibility of the creation.</param>
/// <param name="IsWorkInProgress">A value indicating whether the creation is a work in progress.</param>
/// <param name="Tools">The tools the creation was made with, in the order they were used.</param>
/// <param name="Tags">The tags of the creation.</param>
public sealed record CreationSaveRequest(
    CreationKind Kind,
    string Title,
    string Slug,
    string? Description,
    DateTimeOffset PublishedAt,
    Visibility Visibility,
    bool IsWorkInProgress,
    List<string> Tools,
    List<string> Tags);
