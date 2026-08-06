using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for interacting with projects.
/// </summary>
internal sealed class ProjectService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    /// <param name="markdownPipeline">The Markdown pipeline.</param>
    public ProjectService(IDbContextFactory<AppDbContext> dbContextFactory, MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _markdownPipeline = markdownPipeline;
    }

    /// <summary>
    ///     Gets the description of the specified project.
    /// </summary>
    /// <param name="project">The project whose description to get.</param>
    /// <returns>The description of the specified project.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="project" /> is <see langword="null" />.</exception>
    public string GetDescription(Project project)
    {
        return Markdig.Markdown.ToHtml(project.Description, _markdownPipeline);
    }

    /// <summary>
    ///     Gets all projects.
    /// </summary>
    /// <returns>A read-only list of projects.</returns>
    public IReadOnlyList<Project> GetAllProjects()
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.Projects.OrderBy(p => p.Rank).ThenBy(p => p.Name)];
    }

    /// <summary>
    ///     Gets the programming languages used in the specified project.
    /// </summary>
    /// <param name="project">The project whose languages to return.</param>
    /// <returns>A read only view of the languages.</returns>
    public IReadOnlyList<ProgrammingLanguage> GetProgrammingLanguages(Project project)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return
        [
            .. project.Languages
                .Select(l => context.ProgrammingLanguages.Find(l) ?? new ProgrammingLanguage { Name = l.Titleize() })
        ];
    }

    /// <summary>
    ///     Gets all projects with the specified status.
    /// </summary>
    /// <param name="status">The status of the projects to get.</param>
    /// <returns>A read-only list of projects with the specified status.</returns>
    public IReadOnlyList<Project> GetProjects(ProjectStatus status = ProjectStatus.Ongoing)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.Projects.Where(p => p.Status == status).OrderBy(p => p.Rank).ThenBy(p => p.Name)];
    }

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
    public bool TryGetProject(Guid id, [NotNullWhen(true)] out Project? project)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        project = context.Projects.Find(id);
        return project is not null;
    }

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
    public bool TryGetProject(string slug, [NotNullWhen(true)] out Project? project)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        project = context.Projects.FirstOrDefault(p => p.Slug == slug);
        return project is not null;
    }
}
