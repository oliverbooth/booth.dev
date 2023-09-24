namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a project.
/// </summary>
public interface IProject
{
    /// <summary>
    ///     Gets the description of the project.
    /// </summary>
    /// <value>The description of the project.</value>
    string Description { get; }

    /// <summary>
    ///     Gets the URL of the hero image.
    /// </summary>
    /// <value>The URL of the hero image.</value>
    string HeroUrl { get; }

    /// <summary>
    ///     Gets the ID of the project.
    /// </summary>
    /// <value>The ID of the project.</value>
    Guid Id { get; }

    /// <summary>
    ///     Gets the name of the project.
    /// </summary>
    /// <value>The name of the project.</value>
    string Name { get; }

    /// <summary>
    ///     Gets the rank of the project.
    /// </summary>
    /// <value>The rank of the project.</value>
    int Rank { get; }

    /// <summary>
    ///     Gets the host of the project.
    /// </summary>
    /// <value>The host of the project.</value>
    string? RemoteTarget { get; }

    /// <summary>
    ///     Gets the URL of the project.
    /// </summary>
    /// <value>The URL of the project.</value>
    string? RemoteUrl { get; }

    /// <summary>
    ///     Gets the slug of the project.
    /// </summary>
    /// <value>The slug of the project.</value>
    string Slug { get; }

    /// <summary>
    ///     Gets the status of the project.
    /// </summary>
    /// <value>The status of the project.</value>
    ProjectStatus Status { get; }
}
