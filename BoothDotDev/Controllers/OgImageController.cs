using System.Globalization;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;
using BoothDotDev.Services;
using DEDrake;
using Microsoft.AspNetCore.Mvc;

namespace BoothDotDev.Controllers;

/// <summary>
///     Represents the controller responsible for rendering and caching Open Graph preview images.
/// </summary>
/// <remarks>
///     Every image is rendered once and then cached to disk under the CDN mount, keyed by content ID and hue rather than
///     slug so the URL stays stable across route/slug changes and content edits. For content types that expose an
///     <c>UpdatedAt</c>, the cached file is regenerated once it's older than the content itself; for types that
///     don't (<see cref="Project" />, <see cref="Creation" />), the cache is
///     effectively permanent until the file is deleted by hand.
/// </remarks>
[ApiController]
[Route("og")]
public sealed class OgImageController : ControllerBase
{
    private readonly BlogPostService _blogPostService;
    private readonly CreationService _creationService;
    private readonly DevChallengeService _devChallengeService;
    private readonly MarkdownRenderingService _markdownRenderingService;
    private readonly NoteService _noteService;
    private readonly OgImageService _ogImageService;
    private readonly ProjectService _projectService;
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OgImageController" /> class.
    /// </summary>
    public OgImageController(
        OgImageService ogImageService,
        MarkdownRenderingService markdownRenderingService,
        BlogPostService blogPostService,
        DevChallengeService devChallengeService,
        NoteService noteService,
        TutorialService tutorialService,
        ProjectService projectService,
        CreationService creationService)
    {
        _ogImageService = ogImageService;
        _markdownRenderingService = markdownRenderingService;
        _blogPostService = blogPostService;
        _devChallengeService = devChallengeService;
        _noteService = noteService;
        _tutorialService = tutorialService;
        _projectService = projectService;
        _creationService = creationService;
    }

    /// <summary>
    ///     Gets the generic branded card used by pages with no specific content of their own (Home, Archive, About, ...).
    /// </summary>
    [HttpGet("site.png")]
    public IActionResult GetSiteCard()
    {
        return ServeCached("site", "site", PaletteHue.Brand, null,
            () => _ogImageService.RenderCard(PaletteHue.Brand, "BOOTH.DEV", Strings.MyName, Strings.Tagline));
    }

    /// <summary>
    ///     Gets the card for a blog post.
    /// </summary>
    [HttpGet("blog/{id:guid}.png")]
    public IActionResult GetBlogCard(Guid id)
    {
        var result = _blogPostService.GetPost(id, true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var post = result.Value;
        var hue = post.Color ?? _blogPostService.GetCategory(post.CategoryId)?.EffectiveColor ?? PaletteHue.Brand;
        var description = _markdownRenderingService.RenderPlainTextExcerpt(post, out _);
        return ServeCached("blog", id.ToString("N"), hue, post.UpdatedAt ?? post.PublishedAt,
            () => _ogImageService.RenderCard(hue, "POST", post.Title, description));
    }

    /// <summary>
    ///     Gets the card for a tutorial article.
    /// </summary>
    [HttpGet("tutorial/{id:guid}.png")]
    public IActionResult GetTutorialCard(Guid id)
    {
        var result = _tutorialService.GetArticle(id, true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var article = result.Value;
        var folder = _tutorialService.GetFolder(article.Folder);
        var hue = article.Color ?? (folder.IsSuccess ? folder.Value.EffectiveColor : PaletteHue.Mint);
        var subtitle = folder.IsSuccess
            ? SeriesLabel(article, folder.Value)
            : _markdownRenderingService.RenderPlainTextExcerpt(article, out _);
        return ServeCached("tutorial", id.ToString("N"), hue, article.UpdatedAt ?? article.PublishedAt,
            () => _ogImageService.RenderCard(hue, "TUTORIAL", article.Title, subtitle));
    }

    /// <summary>
    ///     Gets the card for a dev challenge.
    /// </summary>
    [HttpGet("challenge/{id}.png")]
    public IActionResult GetChallengeCard(string id)
    {
        ShortGuid challengeId;
        try
        {
            challengeId = ShortGuid.Parse(id);
        }
        catch (FormatException)
        {
            return NotFound();
        }

        var result = _devChallengeService.GetChallengeById(challengeId, true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var challenge = result.Value;
        var description = _markdownRenderingService.RenderPlainTextExcerpt(challenge, out _);
        return ServeCached("challenge", ((Guid)challenge.Id).ToString("N"), challenge.Hue,
            challenge.UpdatedAt ?? challenge.PublishedAt,
            () => _ogImageService.RenderCard(challenge.Hue, "CHALLENGE", challenge.Title, description));
    }

    /// <summary>
    ///     Gets the card for a note.
    /// </summary>
    [HttpGet("note/{id:guid}.png")]
    public IActionResult GetNoteCard(Guid id)
    {
        var result = _noteService.GetNoteById(id, true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var note = result.Value;
        var subtitle = $"a quick note · {note.PublishedAt.ToString("MMM yyyy", CultureInfo.InvariantCulture)}";
        return ServeCached("note", id.ToString("N"), PaletteHue.Sun, note.UpdatedAt ?? note.PublishedAt,
            () => _ogImageService.RenderCard(PaletteHue.Sun, "NOTE", note.Title, subtitle));
    }

    /// <summary>
    ///     Gets the card for a project devlog entry.
    /// </summary>
    [HttpGet("devlog/{id:guid}.png")]
    public IActionResult GetDevlogCard(Guid id)
    {
        var result = _projectService.GetDevlogById(id, true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var devlog = result.Value;
        var project = _projectService.GetProject(devlog.ProjectId);
        var date = devlog.PublishedAt.ToString("MMM yyyy", CultureInfo.InvariantCulture);
        var subtitle = project.IsSuccess ? $"{project.Value.Name} · devlog · {date}" : $"devlog · {date}";
        return ServeCached("devlog", id.ToString("N"), PaletteHue.Sky, devlog.UpdatedAt ?? devlog.PublishedAt,
            () => _ogImageService.RenderCard(PaletteHue.Sky, "DEVLOG", devlog.Title, subtitle));
    }

    /// <summary>
    ///     Gets the card for a project.
    /// </summary>
    [HttpGet("project/{id:guid}.png")]
    public IActionResult GetProjectCard(Guid id)
    {
        var result = _projectService.GetProject(id);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var project = result.Value;
        var description = _markdownRenderingService.RenderPlainTextPreview(project.Description);
        return ServeCached("project", id.ToString("N"), PaletteHue.Sky, null,
            () => _ogImageService.RenderCard(PaletteHue.Sky, project.Type.Label, project.Name, description));
    }

    /// <summary>
    ///     Gets the card for an artwork item.
    /// </summary>
    [HttpGet("artwork/{id:guid}.png")]
    public IActionResult GetArtworkCard(Guid id)
    {
        var result = _creationService.GetCreation(id, true);
        return GetCreationCard(result.IsFailed ? null : result.Value);
    }

    /// <summary>
    ///     Gets the card for a music item.
    /// </summary>
    [HttpGet("music/{id:guid}.png")]
    public IActionResult GetMusicCard(Guid id)
    {
        var result = _creationService.GetCreation(id, true);
        return GetCreationCard(result.IsFailed ? null : result.Value);
    }

    private IActionResult GetCreationCard(Creation? item)
    {
        if (item is null)
        {
            return NotFound();
        }

        var hue = item.Kind.ToHue();
        var description = string.IsNullOrWhiteSpace(item.Description)
            ? null
            : _markdownRenderingService.RenderPlainTextPreview(item.Description);
        return ServeCached(item.IsMusic ? "music" : "artwork", item.Id.ToString("N"), hue, null,
            () => _ogImageService.RenderCard(hue, item.Kind.ToLabel(), item.Title, description));
    }

    /// <summary>
    ///     Builds the line under a tutorial's title: its folder, and where it falls in its series when it has one.
    /// </summary>
    private string SeriesLabel(TutorialArticle article, TutorialFolder folder)
    {
        if (!article.HasOtherParts)
        {
            return folder.Title;
        }

        // walk back to the first part, then forward through the rest; the set guards against a malformed cycle
        var first = article;
        var visited = new HashSet<Guid> { article.Id };
        while (first.PreviousPart is { } previousId
               && _tutorialService.GetArticle(previousId) is { IsSuccess: true } previous
               && visited.Add(previous.Value.Id))
        {
            first = previous.Value;
        }

        var parts = new List<TutorialArticle> { first };
        visited = [first.Id];
        var cursor = first;
        while (cursor.NextPart is { } nextId
               && _tutorialService.GetArticle(nextId) is { IsSuccess: true } next
               && visited.Add(next.Value.Id))
        {
            parts.Add(next.Value);
            cursor = next.Value;
        }

        parts = [.. parts.Where(part => part.Id == article.Id || part.Visibility == Visibility.Published)];
        var number = parts.FindIndex(part => part.Id == article.Id) + 1;
        return parts.Count > 1 ? $"{folder.Title} · part {number} of {parts.Count}" : folder.Title;
    }

    /// <summary>
    ///     Serves a cached card if one exists and is still fresh, otherwise renders, caches, and serves a fresh one.
    /// </summary>
    /// <param name="type">The content type, used as the cache sub-directory.</param>
    /// <param name="key">The content's own ID, used as the cache filename.</param>
    /// <param name="hue">The hue the card is drawn in, so recolouring content renders a new card.</param>
    /// <param name="contentUpdatedAt">
    ///     The content's last-modified timestamp, or <see langword="null" /> for content with no such concept - the
    ///     cached file is then treated as permanently fresh once it exists.
    /// </param>
    /// <param name="render">Renders a fresh card, only invoked on a cache miss.</param>
    private IActionResult ServeCached(string type, string key, PaletteHue hue, DateTimeOffset? contentUpdatedAt,
        Func<byte[]> render)
    {
        var cachePath = Path.Combine(CdnPaths.GetRoot(), "og", OgImageService.TemplateVersion, type,
            $"{key}-{hue.ToDataHue()}.png");
        var isFresh = System.IO.File.Exists(cachePath) &&
                      (contentUpdatedAt is null ||
                       System.IO.File.GetLastWriteTimeUtc(cachePath) >= contentUpdatedAt.Value.UtcDateTime);

        if (!isFresh)
        {
            var png = render();
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            System.IO.File.WriteAllBytes(cachePath, png);
        }

        Response.Headers.CacheControl = "public, max-age=3600";
        return PhysicalFile(cachePath, "image/png");
    }
}
