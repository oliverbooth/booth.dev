using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for interacting with projects.
/// </summary>
public sealed class ProjectService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly MarkdownRenderingService _markdownRenderingService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    public ProjectService(IDbContextFactory<AppDbContext> dbContextFactory, MarkdownRenderingService markdownRenderingService)
    {
        _dbContextFactory = dbContextFactory;
        _markdownRenderingService = markdownRenderingService;
    }

    /// <summary>
    ///     Gets the description of the specified project.
    /// </summary>
    /// <param name="project">The project whose description to get.</param>
    /// <returns>The description of the specified project.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="project" /> is <see langword="null" />.</exception>
    public string GetDescription(Project project)
    {
        // TODO we need dates on projects, CDN resolver can't just use today. fix this!!!
        return _markdownRenderingService.Render(project.Description, project.Id, DateTimeOffset.UtcNow, "projects");
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
    ///     Gets all devlogs for the specified project.
    /// </summary>
    /// <param name="project">The project to get devlogs for.</param>
    /// <returns>A read-only list of devlogs for the specified project.</returns>
    public IReadOnlyList<ProjectDevlog> GetDevlogs(Project project)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.DevLogs.Where(d => d.ProjectId == project.Id).OrderByDescending(d => d.Published)];
    }
    
    /// <summary>
    ///     Gets the count of projects.
    /// </summary>
    /// <returns>The count of projects.</returns>
    public int GetProjectCount()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.Projects.Count();
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
    ///     Returns the next devlog from the specified devlog.
    /// </summary>
    /// <param name="devlog">The devlog whose next entry to return.</param>
    /// <returns>The next devlog from the specified devlog.</returns>
    public ProjectDevlog? GetNextDevlog(ProjectDevlog devlog)
    {
        if (devlog is null)
        {
            throw new ArgumentNullException(nameof(devlog));
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.DevLogs
            .Where(p => p.ProjectId == devlog.ProjectId)
            .OrderBy(post => post.Published)
            .FirstOrDefault(post => post.Published > devlog.Published);
    }

    /// <summary>
    ///     Returns the previous devlog from the specified devlog.
    /// </summary>
    /// <param name="devlog">The devlog whose previous entry to return.</param>
    /// <returns>The previous devlog from the specified devlog.</returns>
    public ProjectDevlog? GetPreviousDevlog(ProjectDevlog devlog)
    {
        if (devlog is null)
        {
            throw new ArgumentNullException(nameof(devlog));
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.DevLogs
            .Where(p => p.ProjectId == devlog.ProjectId)
            .OrderByDescending(post => post.Published)
            .FirstOrDefault(post => post.Published < devlog.Published);
    }

    /// <summary>
    ///     Returns the most recent devlogs, limited to the specified count.
    /// </summary>
    /// <param name="count">The number of devlogs to return.</param>
    /// <returns>A read-only list of the most recent devlogs.</returns>
    public IReadOnlyList<ProjectDevlog> GetRecentDevlogs(int count)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.DevLogs
            .Where(p => p.Visibility == Visibility.Published)
            .OrderByDescending(p => p.Published)
            .Take(count)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    ///     Attempts to find a devlog entry for the specified project and slug.
    /// </summary>
    /// <param name="project">The project to search for the devlog entry.</param>
    /// <param name="slug">The slug of the devlog entry.</param>
    /// <param name="devlog">
    ///     When this method returns, contains the devlog entry associated with the specified project and slug, if found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if a devlog entry with the specified project and slug is found; otherwise, <see langword="false" />.</returns>
    public bool TryGetDevlog(Project project, string slug, [NotNullWhen(true)] out ProjectDevlog? devlog)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        devlog = context.DevLogs.FirstOrDefault(d => d.ProjectId == project.Id && d.Slug == slug);
        return devlog != null;
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
