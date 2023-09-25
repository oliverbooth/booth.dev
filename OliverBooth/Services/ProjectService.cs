using System.Diagnostics.CodeAnalysis;
using Markdig;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service for interacting with projects.
/// </summary>
internal sealed class ProjectService : IProjectService
{
    private readonly IDbContextFactory<WebContext> _dbContextFactory;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    /// <param name="markdownPipeline">The Markdown pipeline.</param>
    public ProjectService(IDbContextFactory<WebContext> dbContextFactory, MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _markdownPipeline = markdownPipeline;
    }

    /// <inheritdoc />
    public string GetDescription(IProject project)
    {
        return Markdig.Markdown.ToHtml(project.Description, _markdownPipeline);
    }

    /// <inheritdoc />
    public IReadOnlyList<IProject> GetAllProjects()
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        return context.Projects.OrderBy(p => p.Rank).ThenBy(p => p.Name).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<IProject> GetProjects(ProjectStatus status = ProjectStatus.Ongoing)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        return context.Projects.Where(p => p.Status == status).OrderBy(p => p.Rank).ThenBy(p => p.Name).ToArray();
    }

    /// <inheritdoc />
    public bool TryGetProject(Guid id, [NotNullWhen(true)] out IProject? project)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        project = context.Projects.Find(id);
        return project is not null;
    }

    /// <inheritdoc />
    public bool TryGetProject(string slug, [NotNullWhen(true)] out IProject? project)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        project = context.Projects.FirstOrDefault(p => p.Slug == slug);
        return project is not null;
    }
}
