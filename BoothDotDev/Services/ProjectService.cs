using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Models;
using BoothDotDev.Common.Services;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for interacting with projects.
/// </summary>
internal sealed class ProjectService : IProjectService
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

    /// <inheritdoc />
    public string GetDescription(IProject project)
    {
        return Markdig.Markdown.ToHtml(project.Description, _markdownPipeline);
    }

    /// <inheritdoc />
    public IReadOnlyList<IProject> GetAllProjects()
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.Projects.OrderBy(p => p.Rank).ThenBy(p => p.Name)];
    }

    /// <inheritdoc />
    public IReadOnlyList<IProgrammingLanguage> GetProgrammingLanguages(IProject project)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return
        [
            .. project.Languages
                .Select(l => context.ProgrammingLanguages.Find(l) ?? new ProgrammingLanguage { Name = l.Titleize() })
        ];
    }

    /// <inheritdoc />
    public IReadOnlyList<IProject> GetProjects(ProjectStatus status = ProjectStatus.Ongoing)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.Projects.Where(p => p.Status == status).OrderBy(p => p.Rank).ThenBy(p => p.Name)];
    }

    /// <inheritdoc />
    public bool TryGetProject(Guid id, [NotNullWhen(true)] out IProject? project)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        project = context.Projects.Find(id);
        return project is not null;
    }

    /// <inheritdoc />
    public bool TryGetProject(string slug, [NotNullWhen(true)] out IProject? project)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        project = context.Projects.FirstOrDefault(p => p.Slug == slug);
        return project is not null;
    }
}
