namespace BoothDotDev.Data;

/// <summary>
///     Represents a request to create or update a project.
/// </summary>
/// <param name="Name">The name of the project.</param>
/// <param name="Slug">The slug of the project.</param>
/// <param name="Tagline">The tagline of the project, if it has one.</param>
/// <param name="Description">The description of the project.</param>
/// <param name="Details">The details of the project.</param>
/// <param name="Languages">The set of languages used for this project.</param>
/// <param name="Tags">The tags of the project.</param>
/// <param name="Rank">The rank of the project.</param>
/// <param name="Status">The status of the project.</param>
/// <param name="Type">The type of the project.</param>
/// <param name="CreatedAt">The date and time the project was created.</param>
public sealed record ProjectSaveRequest(
    string Name,
    string Slug,
    string? Tagline,
    string Description,
    string Details,
    List<string> Languages,
    List<string> Tags,
    int Rank,
    ProjectStatus Status,
    ProjectType Type,
    DateTimeOffset CreatedAt);
