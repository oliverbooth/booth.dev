using System.Diagnostics.CodeAnalysis;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service for interacting with projects.
/// </summary>
public interface IProjectService
{
    /// <summary>
    ///     Gets the description of the specified project.
    /// </summary>
    /// <param name="project">The project whose description to get.</param>
    /// <returns>The description of the specified project.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="project" /> is <see langword="null" />.</exception>
    string GetDescription(IProject project);

    /// <summary>
    ///     Gets all projects.
    /// </summary>
    /// <returns>A read-only list of projects.</returns>
    IReadOnlyList<IProject> GetAllProjects();

    /// <summary>
    ///     Gets all projects with the specified status.
    /// </summary>
    /// <param name="status">The status of the projects to get.</param>
    /// <returns>A read-only list of projects with the specified status.</returns>
    IReadOnlyList<IProject> GetProjects(ProjectStatus status = ProjectStatus.Ongoing);

    /// <summary>
    ///     Attempts to find a project with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="project">
    ///     When this method returns, contains the project associated with the specified ID, if the project is found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a project with the specified ID is found; otherwise, <see langword="false" />.
    /// </returns>
    bool TryGetProject(Guid id, [NotNullWhen(true)] out IProject? project);

    /// <summary>
    ///     Attempts to find a project with the specified slug.
    /// </summary>
    /// <param name="slug">The slug of the project.</param>
    /// <param name="project">
    ///     When this method returns, contains the project associated with the specified slug, if the project is found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a project with the specified slug is found; otherwise, <see langword="false" />.
    /// </returns>
    bool TryGetProject(string slug, [NotNullWhen(true)] out IProject? project);
}
