using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown.Link;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for interacting with projects.
/// </summary>
public sealed class ProjectService
{
    private const string ProjectArea = "projects";

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly MarkdownRenderingService _markdownRenderingService;
    private readonly CdnMediaService _cdnMediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    /// <param name="markdownRenderingService">The Markdown rendering service.</param>
    /// <param name="cdnMediaService">The <see cref="CdnMediaService" />.</param>
    public ProjectService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        MarkdownRenderingService markdownRenderingService,
        CdnMediaService cdnMediaService)
    {
        _dbContextFactory = dbContextFactory;
        _markdownRenderingService = markdownRenderingService;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets the description of the specified project.
    /// </summary>
    /// <param name="project">The project whose description to get.</param>
    /// <returns>The description of the specified project.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="project" /> is <see langword="null" />.</exception>
    public string GetDescription(Project project)
    {
        return _markdownRenderingService.Render(project.Description, project.Id, project.CreatedAt, ProjectArea);
    }

    /// <summary>
    ///     Gets the CDN URL of the specified project's hero image.
    /// </summary>
    /// <param name="project">The project whose hero image URL to get.</param>
    /// <returns>The hero image's CDN URL, or <see langword="null" /> if the project has no hero image.</returns>
    public string? GetHeroUrl(Project project)
    {
        if (string.IsNullOrEmpty(project.HeroUrl))
        {
            return null;
        }

        var kind = CdnMediaResolver.ResolveMediaKind(project.HeroUrl);
        return CdnMediaResolver.BuildCdnUrl(ProjectArea, kind, project.CreatedAt, project.Id, project.HeroUrl);
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
    ///     Gets the number of devlogs for the specified project.
    /// </summary>
    /// <param name="project">The project to get devlogs for.</param>
    /// <returns>The number of devlogs for the specified project.</returns>
    public int GetDevlogCount(Option<Project> project = default)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return project.HasValue
            ? context.DevLogs.Count(d => d.ProjectId == project.ValueOr((Project)null!).Id)
            : context.DevLogs.Count();
    }

    /// <summary>
    ///     Gets all devlogs for the specified project.
    /// </summary>
    /// <param name="project">The project to get devlogs for.</param>
    /// <returns>A read-only list of devlogs for the specified project.</returns>
    public IReadOnlyList<ProjectDevlog> GetDevlogs(Project project)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.DevLogs.Where(d => d.ProjectId == project.Id).OrderByDescending(d => d.PublishedAt)];
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
            .OrderBy(post => post.PublishedAt)
            .FirstOrDefault(post => post.PublishedAt > devlog.PublishedAt);
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
            .OrderByDescending(post => post.PublishedAt)
            .FirstOrDefault(post => post.PublishedAt < devlog.PublishedAt);
    }

    /// <summary>
    ///     Returns the most recent devlogs, limited to the specified count.
    /// </summary>
    /// <param name="searchOptions">The options for searching and retrieving devlogs.</param>
    /// <returns>A read-only list of the most recent devlogs.</returns>
    public IReadOnlyList<ProjectDevlog> GetRecentDevlogs(ActivitySearchOptions searchOptions)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var devlogs = context.DevLogs.AsQueryable();

        if (searchOptions.Visibility != Visibility.None)
        {
            devlogs = devlogs.Where(p => p.Visibility == searchOptions.Visibility);
        }

        var ordered = searchOptions.SortStrategy switch
        {
            ActivitySortStrategy.Published => devlogs.OrderByDescending(p => p.PublishedAt),
            ActivitySortStrategy.Updated => devlogs.OrderByDescending(p => p.UpdatedAt ?? p.PublishedAt),
            _ => throw new ArgumentOutOfRangeException(nameof(searchOptions), searchOptions.SortStrategy, "Unknown sort strategy")
        };

        return [.. ordered.Take(searchOptions.Count)];
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

    /// <summary>
    ///     Gets a project by its ID.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <returns>A <see cref="Result{T}" /> containing the project if found; otherwise, an error result.</returns>
    public Result<Project> GetProject(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var project = context.Projects.Find(id);
        return project is not null ? project : Result.Fail($"The project with ID {id} was not found");
    }

    /// <summary>
    ///     Creates a new project.
    /// </summary>
    /// <param name="request">The project's fields.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created project.</returns>
    public Result<Project> CreateProject(ProjectSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var project = new Project();
        ApplyProjectRequest(project, request);

        context.Projects.Add(project);
        context.SaveChanges();

        return project;
    }

    /// <summary>
    ///     Updates an existing project.
    /// </summary>
    /// <param name="id">The ID of the project to update.</param>
    /// <param name="request">The project's new fields.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated project, or an error if no project with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<Project> UpdateProject(Guid id, ProjectSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var project = context.Projects.Find(id);

        if (project is null)
        {
            return Result.Fail($"The project with ID {id} was not found");
        }

        ApplyProjectRequest(project, request);
        context.SaveChanges();

        return project;
    }

    /// <summary>
    ///     Deletes a project. The project must not have any devlog entries.
    /// </summary>
    /// <param name="id">The ID of the project to delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if the project was not found or still has
    ///     devlog entries.
    /// </returns>
    public Result DeleteProject(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var project = context.Projects.Find(id);

        if (project is null)
        {
            return Result.Fail($"The project with ID {id} was not found");
        }

        var hasDevlogs = context.DevLogs.Any(d => d.ProjectId == id);
        if (hasDevlogs)
        {
            return Result.Fail("This project has devlog entries. Delete them first.");
        }

        if (!string.IsNullOrEmpty(project.HeroUrl))
        {
            _cdnMediaService.DeleteAllMedia(id, project.CreatedAt, ProjectArea);
        }

        context.Projects.Remove(project);
        context.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Applies the fields of a <see cref="ProjectSaveRequest" /> onto a <see cref="Project" />.
    /// </summary>
    /// <param name="project">The project to apply the request to.</param>
    /// <param name="request">The request containing the fields to apply.</param>
    private static void ApplyProjectRequest(Project project, ProjectSaveRequest request)
    {
        project.Name = request.Name;
        project.Slug = request.Slug;
        project.Tagline = request.Tagline;
        project.Description = request.Description;
        project.Details = request.Details;
        project.HeroUrl = request.HeroUrl;
        project.Languages = request.Languages;
        project.Rank = request.Rank;
        project.RemoteUrl = request.RemoteUrl;
        project.RemoteTarget = request.RemoteTarget;
        project.Status = request.Status;
        project.Type = request.Type;
        project.CreatedAt = request.CreatedAt.ToUniversalTime();
    }
}
