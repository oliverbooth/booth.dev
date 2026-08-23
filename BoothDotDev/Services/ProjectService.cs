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
    private const string DevlogArea = "devlog";

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
            ? context.DevLogs.Count(d => d.TrashedAt == null && d.ProjectId == project.ValueOr((Project)null!).Id)
            : context.DevLogs.Count(d => d.TrashedAt == null);
    }

    /// <summary>
    ///     Gets all non-trashed devlogs for the specified project.
    /// </summary>
    /// <param name="project">The project to get devlogs for.</param>
    /// <param name="visibility">
    ///     The visibility of the devlogs to retrieve. If set to <see cref="Visibility.None" />, every devlog is
    ///     returned regardless of visibility - only the admin listing should pass this.
    /// </param>
    /// <returns>A read-only list of devlogs for the specified project, newest-published first.</returns>
    public IReadOnlyList<ProjectDevlog> GetDevlogs(Project project, Visibility visibility = Visibility.None)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var devlogs = context.DevLogs.Include(d => d.CurrentDraft).Where(d => d.TrashedAt == null && d.ProjectId == project.Id);

        if (visibility != Visibility.None)
        {
            devlogs = devlogs.Where(d => d.CurrentDraft!.Visibility == visibility);
        }

        return [.. devlogs.OrderByDescending(d => d.PublishedAt)];
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
            .Include(d => d.CurrentDraft)
            .Where(p => p.TrashedAt == null && p.ProjectId == devlog.ProjectId)
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
            .Include(d => d.CurrentDraft)
            .Where(p => p.TrashedAt == null && p.ProjectId == devlog.ProjectId)
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
        var devlogs = context.DevLogs.Include(d => d.CurrentDraft).Where(d => d.TrashedAt == null);

        if (searchOptions.Visibility != Visibility.None)
        {
            devlogs = devlogs.Where(p => p.CurrentDraft!.Visibility == searchOptions.Visibility);
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
    ///     Gets all trashed devlogs for the specified project, newest-trashed first.
    /// </summary>
    /// <param name="project">The project whose trashed devlogs to get.</param>
    /// <returns>A read-only list of the project's trashed devlogs.</returns>
    public IReadOnlyList<ProjectDevlog> GetTrashedDevlogs(Project project)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.DevLogs.Include(d => d.CurrentDraft)
                .Where(d => d.TrashedAt != null && d.ProjectId == project.Id)
                .OrderByDescending(d => d.TrashedAt)
        ];
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
    /// <param name="includeTrashed">
    ///     Whether to include the devlog entry if it's trashed. Only the admin editor should pass <see langword="true" /> —
    ///     every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns><see langword="true" /> if a devlog entry with the specified project and slug is found; otherwise, <see langword="false" />.</returns>
    public bool TryGetDevlog(Project project, string slug, [NotNullWhen(true)] out ProjectDevlog? devlog, bool includeTrashed = false)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        devlog = context.DevLogs.Include(d => d.CurrentDraft)
            .FirstOrDefault(d => d.ProjectId == project.Id && d.Slug == slug && (includeTrashed || d.TrashedAt == null));
        return devlog != null;
    }

    /// <summary>
    ///     Gets a devlog entry by its ID.
    /// </summary>
    /// <param name="id">The ID of the devlog entry.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the devlog entry if it's trashed. Only the admin editor should pass
    ///     <see langword="true" /> — every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the devlog entry if found; otherwise, an error result.</returns>
    public Result<ProjectDevlog> GetDevlogById(Guid id, bool includeTrashed = false)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var devlog = context.DevLogs.Include(d => d.CurrentDraft).FirstOrDefault(d => d.Id == id);

        if (devlog is null || (devlog.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail($"The devlog entry with ID {id} was not found");
        }

        return devlog;
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
    ///     Deletes a project. The project must not have any devlog entries, trashed or not - permanently delete
    ///     them first.
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

        // Counts *every* devlog, including trashed ones, not just d.TrashedAt == null - the devlog->project
        // foreign key is RESTRICT, so the database would reject this delete anyway if any row (trashed or
        // not) still referenced this project. Surfacing that as a friendly error here means the admin never
        // sees a raw DbUpdateException; they're told to permanently delete the devlogs first instead.
        var hasDevlogs = context.DevLogs.Any(d => d.ProjectId == id);
        if (hasDevlogs)
        {
            return Result.Fail("This project has devlog entries. Permanently delete them first.");
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
    ///     Creates a new devlog entry, along with its first draft, which immediately becomes the entry's current
    ///     draft.
    /// </summary>
    /// <param name="request">The devlog entry's parent-level fields and the content of its first draft.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created devlog entry.</returns>
    public Result<ProjectDevlog> CreateDevlog(ProjectDevlogSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var devlog = new ProjectDevlog
        {
            ProjectId = request.ProjectId,
            Slug = request.Slug,
            PublishedAt = request.PublishedAt.ToUniversalTime(),
            EnableComments = request.EnableComments
        };

        // Two SaveChanges calls, not one: ProjectDevlog -> ProjectDevlogDraft (via ProjectDevlogId) and
        // ProjectDevlogDraft -> ProjectDevlog (via CurrentDraftId) form a cycle between two rows that are
        // both new, which EF can't resolve in a single call even though CurrentDraftId is nullable.
        context.DevLogs.Add(devlog);
        context.SaveChanges();

        var draft = NewDraft(devlog.Id, request.Content);
        context.ProjectDevlogDrafts.Add(draft);
        devlog.CurrentDraftId = draft.Id;
        context.SaveChanges();

        return devlog;
    }

    /// <summary>
    ///     Saves a new draft of an existing devlog entry, without publishing it.
    /// </summary>
    /// <param name="id">The ID of the devlog entry to save a draft for.</param>
    /// <param name="request">The devlog entry's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the devlog entry the draft was saved for, or an error if no entry with
    ///     the specified <paramref name="id" /> exists.
    /// </returns>
    public Result<ProjectDevlog> SaveDevlogDraft(Guid id, ProjectDevlogSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var devlog = context.DevLogs.Find(id);

        if (devlog is null)
        {
            return Result.Fail($"The devlog entry with ID {id} was not found");
        }

        var draft = NewDraft(devlog.Id, request.Content);
        context.ProjectDevlogDrafts.Add(draft);

        devlog.Slug = request.Slug;
        devlog.PublishedAt = request.PublishedAt.ToUniversalTime();
        devlog.EnableComments = request.EnableComments;

        context.SaveChanges();
        return devlog;
    }

    /// <summary>
    ///     Saves a new draft of an existing devlog entry and publishes it, making it the entry's current draft.
    /// </summary>
    /// <param name="id">The ID of the devlog entry to publish.</param>
    /// <param name="request">The devlog entry's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated devlog entry, or an error if no entry with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<ProjectDevlog> PublishDevlog(Guid id, ProjectDevlogSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var devlog = context.DevLogs.Find(id);

        if (devlog is null)
        {
            return Result.Fail($"The devlog entry with ID {id} was not found");
        }

        var draft = NewDraft(devlog.Id, request.Content);
        context.ProjectDevlogDrafts.Add(draft);

        devlog.Slug = request.Slug;
        devlog.PublishedAt = request.PublishedAt.ToUniversalTime();
        devlog.EnableComments = request.EnableComments;
        devlog.CurrentDraftId = draft.Id;
        devlog.UpdatedAt = DateTimeOffset.UtcNow;

        context.SaveChanges();
        return devlog;
    }

    /// <summary>
    ///     Moves a devlog entry to the trash. It's excluded from every listing and 404s on its public URL, but
    ///     nothing about it is otherwise touched, and it can be restored with <see cref="RestoreDevlog" />.
    /// </summary>
    /// <param name="id">The ID of the devlog entry to trash.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the trashed devlog entry, or an error if no entry with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<ProjectDevlog> TrashDevlog(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var devlog = context.DevLogs.Find(id);

        if (devlog is null)
        {
            return Result.Fail($"The devlog entry with ID {id} was not found");
        }

        devlog.TrashedAt = DateTimeOffset.UtcNow;
        context.SaveChanges();
        return devlog;
    }

    /// <summary>
    ///     Restores a trashed devlog entry, making it visible in listings and on its public URL again.
    /// </summary>
    /// <param name="id">The ID of the devlog entry to restore.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the restored devlog entry, or an error if no entry with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<ProjectDevlog> RestoreDevlog(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var devlog = context.DevLogs.Find(id);

        if (devlog is null)
        {
            return Result.Fail($"The devlog entry with ID {id} was not found");
        }

        devlog.TrashedAt = null;
        context.SaveChanges();
        return devlog;
    }

    /// <summary>
    ///     Permanently deletes a trashed devlog entry - the entry row, every draft in its revision history
    ///     (cascade), and every file it had uploaded to the CDN. This cannot be undone.
    /// </summary>
    /// <param name="id">The ID of the devlog entry to permanently delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if no entry with the specified <paramref name="id" />
    ///     exists or it isn't currently trashed.
    /// </returns>
    public Result PermanentlyDeleteDevlog(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var devlog = context.DevLogs.Find(id);

        if (devlog is null)
        {
            return Result.Fail($"The devlog entry with ID {id} was not found");
        }

        if (devlog.TrashedAt is null)
        {
            return Result.Fail("Only trashed devlog entries can be permanently deleted.");
        }

        _cdnMediaService.DeleteAllMedia(id, devlog.PublishedAt, DevlogArea);

        context.DevLogs.Remove(devlog);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Returns a devlog entry's full draft history, newest first.
    /// </summary>
    /// <param name="id">The ID of the devlog entry whose draft history to return.</param>
    /// <returns>The devlog entry's drafts, newest first.</returns>
    public IReadOnlyList<ProjectDevlogDraft> GetDraftHistory(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.ProjectDevlogDrafts.Where(d => d.ProjectDevlogId == id).OrderByDescending(d => d.CreatedAt)];
    }

    /// <summary>
    ///     Returns a specific draft of the specified devlog entry, for viewing without publishing it.
    /// </summary>
    /// <param name="id">The ID of the devlog entry the draft belongs to.</param>
    /// <param name="draftId">The ID of the draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the requested draft, or an error if it doesn't exist or doesn't belong to
    ///     the specified devlog entry.
    /// </returns>
    public Result<ProjectDevlogDraft> GetDraft(Guid id, Guid draftId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.ProjectDevlogDrafts.Find(draftId);

        if (draft is null || draft.ProjectDevlogId != id)
        {
            return Result.Fail($"Draft '{draftId}' not found for devlog entry '{id}'.");
        }

        return draft;
    }

    /// <summary>
    ///     Returns the newest draft of the specified devlog entry, which may or may not be the entry's current
    ///     (published) draft.
    /// </summary>
    /// <param name="id">The ID of the devlog entry whose newest draft to return.</param>
    /// <returns>A <see cref="Result{T}" /> containing the entry's newest draft, or an error if it has no drafts.</returns>
    public Result<ProjectDevlogDraft> GetNewestDraft(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.ProjectDevlogDrafts.Where(d => d.ProjectDevlogId == id).OrderByDescending(d => d.CreatedAt).FirstOrDefault();

        if (draft is null)
        {
            return Result.Fail($"Devlog entry '{id}' has no drafts.");
        }

        return draft;
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

    /// <summary>
    ///     Builds a new, unsaved draft snapshot for the specified devlog entry.
    /// </summary>
    /// <param name="devlogId">The ID of the devlog entry for which to create a draft.</param>
    /// <param name="content">The content for the new draft.</param>
    /// <returns>The newly created draft.</returns>
    private static ProjectDevlogDraft NewDraft(Guid devlogId, ProjectDevlogDraftContent content)
    {
        return new ProjectDevlogDraft
        {
            ProjectDevlogId = devlogId,
            Title = content.Title,
            Body = content.Body,
            Visibility = content.Visibility
        };
    }
}
