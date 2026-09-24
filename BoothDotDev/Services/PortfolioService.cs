using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown;
using BoothDotDev.Markdown.Link;
using Microsoft.Extensions.Options;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service that lists projects and creations together as <see cref="PortfolioItem" /> cards.
/// </summary>
public sealed class PortfolioService
{
    private const int WaveformBarCount = 18;

    /// <summary>
    ///     The page creations link to. There is no page for an individual creation yet, so they all lead to the portfolio.
    /// </summary>
    private const string CreationPagePath = "/Portfolio/Index";

    /// <summary>
    ///     The order project statuses are listed in. This is not the enum's order, which puts hiatus before past.
    /// </summary>
    private static readonly ProjectStatus[] StatusPriority =
        [ProjectStatus.Ongoing, ProjectStatus.Past, ProjectStatus.Hiatus, ProjectStatus.Retired];

    private readonly string _cdnBaseUrl;
    private readonly CreationService _creationService;
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PortfolioService" /> class.
    /// </summary>
    /// <param name="projectService">The <see cref="ProjectService" />.</param>
    /// <param name="creationService">The <see cref="CreationService" />.</param>
    /// <param name="cdnOptions">The CDN options.</param>
    public PortfolioService(ProjectService projectService, CreationService creationService, IOptions<CdnOptions> cdnOptions)
    {
        _projectService = projectService;
        _creationService = creationService;
        _cdnBaseUrl = cdnOptions.Value.BaseUrl;
    }

    /// <summary>
    ///     Gets the items to feature: the newest creations first, then projects to make up the count.
    /// </summary>
    /// <param name="count">The total number of items to return.</param>
    /// <param name="creationCount">The number of newest creations to lead with.</param>
    /// <returns>
    ///     The newest creations, then projects by status (ongoing, past, hiatus, retired), then rank, then name. Fewer
    ///     creations than <paramref name="creationCount" /> leaves more room for projects.
    /// </returns>
    public IReadOnlyList<PortfolioItem> GetFeatured(int count, int creationCount)
    {
        var creations = GetNewestCreations(creationCount);
        var projects = GetProjects().Take(Math.Max(0, count - creations.Count));
        return [.. creations, .. projects];
    }

    private IEnumerable<PortfolioItem> GetProjects()
    {
        // each status is already ordered by rank then name, so concatenating them in priority order is the whole sort
        return StatusPriority
            .SelectMany(status => _projectService.GetProjects(status))
            .Select(project => ToItem(project));
    }

    private List<PortfolioItem> GetNewestCreations(int creationCount)
    {
        IEnumerable<(DateTimeOffset PublishedAt, PortfolioItem Item)> artwork = _creationService.GetArtworkItems()
            .Select(a => (a.PublishedAt, ToItem(a)));
        IEnumerable<(DateTimeOffset PublishedAt, PortfolioItem Item)> music = _creationService.GetMusicItems()
            .Select(m => (m.PublishedAt, ToItem(m)));

        return [.. artwork.Concat(music).OrderByDescending(x => x.PublishedAt).Take(creationCount).Select(x => x.Item)];
    }

    private PortfolioItem ToItem(Project project)
    {
        return new PortfolioItem
        {
            Kind = PortfolioItemKind.Project,
            Title = project.Name,
            Description = Markdig.Markdown.ToPlainText(project.Description),
            ImageUrl = _projectService.GetHeroUrl(project),
            PagePath = "/Projects/Project",
            RouteValues = new Dictionary<string, string> { ["slug"] = project.Slug },
            Hue = PaletteHue.Sky,
            Label = "code",
            Tags = project.Languages,
            Status = project.Status
        };
    }

    private PortfolioItem ToItem(ArtworkItem artwork)
    {
        var resolver = new CdnMediaResolver(new MarkdownRenderContext(artwork.Id, artwork.PublishedAt), null!, "content",
            _cdnBaseUrl);

        return new PortfolioItem
        {
            Kind = PortfolioItemKind.Artwork,
            Title = artwork.Title,
            Description = artwork.Description,
            ImageUrl = resolver.ResolveCdnUrl(artwork.FileName, MediaKind.Image),
            PagePath = CreationPagePath,
            Hue = PaletteHue.Pink,
            Label = "art",
            Tags = TagsFor(artwork),
            IsWorkInProgress = artwork.IsWorkInProgress
        };
    }

    private static PortfolioItem ToItem(MusicItem music)
    {
        return new PortfolioItem
        {
            Kind = PortfolioItemKind.Music,
            Title = music.Title,
            Description = music.Description,
            PagePath = CreationPagePath,
            Hue = PaletteHue.Mint,
            Label = "music",
            Tags = TagsFor(music),
            IsWorkInProgress = music.IsWorkInProgress,
            WaveformBars = SeededBars(music.Id)
        };
    }

    private static IReadOnlyList<string> TagsFor(CreativeItem item)
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
