using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown;
using BoothDotDev.Markdown.Link;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service that lists projects and creations together as <see cref="PortfolioItem" /> cards, in the order
///     set by their <see cref="PortfolioEntry" />.
/// </summary>
public sealed class PortfolioService
{
    /// <summary>
    ///     The number of featured items the home page shows.
    /// </summary>
    public const int FeaturedLimit = 6;

    private const int WaveformBarCount = 18;

    /// <summary>
    ///     The page creations link to. There is no page for an individual creation yet, so they all lead to the portfolio.
    /// </summary>
    private const string CreationPagePath = "/Portfolio/Index";

    private readonly string _cdnBaseUrl;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PortfolioService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    /// <param name="projectService">The <see cref="ProjectService" />.</param>
    /// <param name="cdnOptions">The CDN options.</param>
    public PortfolioService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        ProjectService projectService,
        IOptions<CdnOptions> cdnOptions)
    {
        _dbContextFactory = dbContextFactory;
        _projectService = projectService;
        _cdnBaseUrl = cdnOptions.Value.BaseUrl;
    }

    /// <summary>
    ///     Gets every listed item that can be shown publicly, in portfolio order.
    /// </summary>
    /// <returns>The items.</returns>
    public IReadOnlyList<PortfolioItem> GetAll()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return [.. ShownEntries(dbContext).OrderBy(e => e.Position).AsEnumerable().Select(ToItem)];
    }

    /// <summary>
    ///     Gets the items featured on the home page, in featured order.
    /// </summary>
    /// <param name="count">The most items to return.</param>
    /// <returns>
    ///     The featured items that can be shown publicly. If none are featured, the first items of the portfolio instead,
    ///     so the home page is never left empty.
    /// </returns>
    public IReadOnlyList<PortfolioItem> GetFeatured(int count = FeaturedLimit)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        PortfolioEntry[] featured =
        [
            .. ShownEntries(dbContext)
                .Where(e => e.FeaturedPosition != null)
                .OrderBy(e => e.FeaturedPosition)
                .Take(count)
        ];

        if (featured.Length == 0)
        {
            featured = [.. ShownEntries(dbContext).OrderBy(e => e.Position).Take(count)];
        }

        return [.. featured.Select(ToItem)];
    }

    /// <summary>
    ///     Gets everything the portfolio admin page shows.
    /// </summary>
    /// <returns>The <see cref="PortfolioAdminView" />.</returns>
    public PortfolioAdminView GetAdminView()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        PortfolioEntry[] entries = [.. WithTargets(dbContext).OrderBy(e => e.Position)];
        PortfolioAdminItem[] listed = [.. entries.Select(ToAdminItem)];
        PortfolioAdminItem[] featured = [.. entries.Where(e => e.FeaturedPosition != null).OrderBy(e => e.FeaturedPosition).Select(ToAdminItem)];

        var unlistedProjects = dbContext.Projects
            .Where(p => !dbContext.PortfolioEntries.Any(e => e.ProjectId == p.Id))
            .OrderBy(p => p.Name)
            .AsEnumerable()
            .Select(p => new PortfolioAdminItem(p.Id, null, p.Name, PortfolioItemKind.Project, PaletteHue.Sky, "code", false, null));

        var unlistedCreations = dbContext.Creations
            .Where(c => c.TrashedAt == null && !dbContext.PortfolioEntries.Any(e => e.CreationId == c.Id))
            .OrderByDescending(c => c.PublishedAt)
            .AsEnumerable()
            .Select(c =>
            {
                var (kind, hue, label) = Describe(c);
                return new PortfolioAdminItem(c.Id, null, c.Title, kind, hue, label, false, null);
            });

        return new PortfolioAdminView(listed, featured, [.. unlistedCreations, .. unlistedProjects]);
    }

    /// <summary>
    ///     Lists a project or creation on the portfolio, after everything already listed.
    /// </summary>
    /// <param name="itemId">The ID of the project or creation.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the item could not be listed.</returns>
    public Result List(Guid itemId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var isProject = dbContext.Projects.Any(p => p.Id == itemId);
        if (!isProject && !dbContext.Creations.Any(c => c.Id == itemId && c.TrashedAt == null))
        {
            return Result.Fail($"No project or creation with ID {itemId} was found");
        }

        if (dbContext.PortfolioEntries.Any(e => e.ProjectId == itemId || e.CreationId == itemId))
        {
            return Result.Ok();
        }

        var position = dbContext.PortfolioEntries.Select(e => (int?)e.Position).Max() + 1 ?? 0;
        dbContext.PortfolioEntries.Add(new PortfolioEntry
        {
            Position = position,
            ProjectId = isProject ? itemId : null,
            CreationId = isProject ? null : itemId
        });
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Removes an entry from the portfolio. The project or creation it stood for is untouched.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the entry could not be removed.</returns>
    public Result Unlist(Guid entryId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var entry = dbContext.PortfolioEntries.Find(entryId);
        if (entry is null)
        {
            return Result.Fail($"The portfolio entry with ID {entryId} was not found");
        }

        dbContext.PortfolioEntries.Remove(entry);
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Features an entry on the home page, after everything already featured.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the entry could not be featured.</returns>
    public Result Feature(Guid entryId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var entry = dbContext.PortfolioEntries.Find(entryId);
        if (entry is null)
        {
            return Result.Fail($"The portfolio entry with ID {entryId} was not found");
        }

        if (entry.FeaturedPosition is null)
        {
            entry.FeaturedPosition = dbContext.PortfolioEntries.Select(e => e.FeaturedPosition).Max() + 1 ?? 0;
            dbContext.SaveChanges();
        }

        return Result.Ok();
    }

    /// <summary>
    ///     Stops featuring an entry on the home page.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the entry could not be unfeatured.</returns>
    public Result Unfeature(Guid entryId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var entry = dbContext.PortfolioEntries.Find(entryId);
        if (entry is null)
        {
            return Result.Fail($"The portfolio entry with ID {entryId} was not found");
        }

        entry.FeaturedPosition = null;
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Reassigns every listed entry's position to match its place in <paramref name="orderedEntryIds" />.
    /// </summary>
    /// <param name="orderedEntryIds">The entry IDs, in their new order.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result Reorder(IReadOnlyList<Guid> orderedEntryIds)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var entries = dbContext.PortfolioEntries.Where(e => orderedEntryIds.Contains(e.Id)).ToDictionary(e => e.Id);

        for (var i = 0; i < orderedEntryIds.Count; i++)
        {
            if (entries.TryGetValue(orderedEntryIds[i], out var entry))
            {
                entry.Position = i;
            }
        }

        dbContext.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Reassigns every featured entry's featured position to match its place in <paramref name="orderedEntryIds" />.
    /// </summary>
    /// <param name="orderedEntryIds">The IDs of the featured entries, in their new order.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result ReorderFeatured(IReadOnlyList<Guid> orderedEntryIds)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var entries = dbContext.PortfolioEntries
            .Where(e => orderedEntryIds.Contains(e.Id) && e.FeaturedPosition != null)
            .ToDictionary(e => e.Id);

        for (var i = 0; i < orderedEntryIds.Count; i++)
        {
            if (entries.TryGetValue(orderedEntryIds[i], out var entry))
            {
                entry.FeaturedPosition = i;
            }
        }

        dbContext.SaveChanges();
        return Result.Ok();
    }

    private static IQueryable<PortfolioEntry> WithTargets(AppDbContext dbContext)
    {
        return dbContext.PortfolioEntries.Include(e => e.Project).Include(e => e.Creation);
    }

    /// <summary>
    ///     Gets the entries that can be shown publicly: every project, and the creations that are published and not
    ///     trashed.
    /// </summary>
    private static IQueryable<PortfolioEntry> ShownEntries(AppDbContext dbContext)
    {
        return WithTargets(dbContext).Where(e =>
            e.Project != null || (e.Creation!.Visibility == Visibility.Published && e.Creation.TrashedAt == null));
    }

    private static (PortfolioItemKind Kind, PaletteHue Hue, string Label) Describe(Creation creation)
    {
        return creation.Kind switch
        {
            CreationKind.Drawing => (PortfolioItemKind.Drawing, PaletteHue.Pink, "drawing"),
            CreationKind.ThreeD => (PortfolioItemKind.ThreeD, PaletteHue.Tangerine, "3d"),
            CreationKind.Music => (PortfolioItemKind.Music, PaletteHue.Mint, "music"),
            _ => throw new InvalidOperationException($"Unknown creation kind {creation.Kind}.")
        };
    }

    private PortfolioAdminItem ToAdminItem(PortfolioEntry entry)
    {
        if (entry.Project is { } project)
        {
            return new PortfolioAdminItem(project.Id, entry.Id, project.Name, PortfolioItemKind.Project, PaletteHue.Sky,
                "code", entry.FeaturedPosition is not null, null);
        }

        var creation = entry.Creation!;
        var (kind, hue, label) = Describe(creation);
        var hiddenReason = creation.TrashedAt is not null ? "in the trash"
            : creation.Visibility != Visibility.Published ? creation.Visibility.ToString().ToLowerInvariant()
            : null;

        return new PortfolioAdminItem(creation.Id, entry.Id, creation.Title, kind, hue, label,
            entry.FeaturedPosition is not null, hiddenReason);
    }

    private PortfolioItem ToItem(PortfolioEntry entry)
    {
        return entry.Project is { } project ? ToItem(project) : ToItem(entry.Creation!);
    }

    private PortfolioItem ToItem(Project project)
    {
        return new PortfolioItem
        {
            Kind = PortfolioItemKind.Project,
            Title = project.Name,
            Description = Markdig.Markdown.ToPlainText(project.Description),
            ImageUrl = _projectService.GetHeroUrl(project),
            PagePath = "/Portfolio/Project",
            RouteValues = new Dictionary<string, string> { ["slug"] = project.Slug },
            Hue = PaletteHue.Sky,
            Label = "code",
            Tags = project.Languages,
            Status = project.Status
        };
    }

    private PortfolioItem ToItem(Creation creation)
    {
        var resolver = new CdnMediaResolver(new MarkdownRenderContext(creation.Id, creation.PublishedAt), null!, "content",
            _cdnBaseUrl);

        var (kind, hue, label) = Describe(creation);

        return new PortfolioItem
        {
            Kind = kind,
            Id = creation.Id,
            PublishedAt = creation.PublishedAt,
            Title = creation.Title,
            Description = PlainText(creation.Description),
            DescriptionMarkdown = creation.Description,
            ImageUrl = creation.IsMusic ? null : resolver.ResolveCdnUrl(creation.FileName, MediaKind.Image),
            AudioUrl = creation.IsMusic ? resolver.ResolveCdnUrl(creation.FileName, MediaKind.Audio) : null,
            Duration = creation.Duration,
            PagePath = CreationPagePath,
            Hue = hue,
            Label = label,
            Tags = TagsFor(creation),
            IsWorkInProgress = creation.IsWorkInProgress,
            WaveformBars = creation.IsMusic ? SeededBars(creation.Id) : []
        };
    }

    private static string? PlainText(string? markdown)
    {
        return string.IsNullOrWhiteSpace(markdown) ? null : Markdig.Markdown.ToPlainText(markdown).Trim();
    }

    private static IReadOnlyList<string> TagsFor(Creation item)
    {
        return string.IsNullOrWhiteSpace(item.MadeWith) ? [] : [item.MadeWith];
    }

    /// <summary>
    ///     Generates the heights of a decorative waveform. It is not the track's real waveform, only a stable-looking
    ///     pattern per item, from the style guide's seeded generator.
    /// </summary>
    private static IReadOnlyList<int> SeededBars(Guid id)
    {
        // bytes 8-15 of a version 7 GUID are random; the leading bytes are a timestamp and would barely differ between items
        long state = BitConverter.ToUInt16(id.ToByteArray(), 14);
        var bars = new int[WaveformBarCount];

        for (var i = 0; i < bars.Length; i++)
        {
            state = ((state * 9301) + 49297) % 233280;
            bars[i] = 10 + (int)Math.Round(state / 233280d * 34);
        }

        return bars;
    }
}
