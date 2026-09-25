using BoothDotDev.Data.Models;

namespace BoothDotDev.Data;

/// <summary>
///     Identifies a project or creation as the owner of media, with what is needed to find its folder on the CDN.
/// </summary>
/// <param name="Id">The ID of the project or creation.</param>
/// <param name="Date">The date its folder is filed under: a project's creation date or a creation's publication date.</param>
/// <param name="Area">The CDN area its folder is in.</param>
public readonly record struct MediaOwner(Guid Id, DateTimeOffset Date, string Area)
{
    /// <summary>
    ///     The CDN area project media is in.
    /// </summary>
    public const string ProjectArea = "projects";

    /// <summary>
    ///     The CDN area creation media is in.
    /// </summary>
    public const string CreationArea = "content";

    /// <summary>
    ///     Gets the owner for a project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns>The <see cref="MediaOwner" />.</returns>
    public static MediaOwner For(Project project)
    {
        return new MediaOwner(project.Id, project.CreatedAt, ProjectArea);
    }

    /// <summary>
    ///     Gets the owner for a creation.
    /// </summary>
    /// <param name="creation">The creation.</param>
    /// <returns>The <see cref="MediaOwner" />.</returns>
    public static MediaOwner For(Creation creation)
    {
        return new MediaOwner(creation.Id, creation.PublishedAt, CreationArea);
    }
}
