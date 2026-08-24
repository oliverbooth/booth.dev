using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Markdown.Link;
using BoothDotDev.Services;
using DEDrake;
using Microsoft.AspNetCore.Mvc;

namespace BoothDotDev.Controllers;

/// <summary>
///     Represents the controller responsible for rendering and caching Open Graph preview images.
/// </summary>
/// <remarks>
///     Every image is rendered once and then cached to disk under the CDN mount, keyed by content ID rather than
///     slug so the URL stays stable across route/slug changes and content edits. For content types that expose an
///     <c>UpdatedAt</c>, the cached file is regenerated once it's older than the content itself; for types that
///     don't (<see cref="Project" />, <see cref="ArtworkItem" />, <see cref="MusicItem" />), the cache is
///     effectively permanent until the file is deleted by hand.
/// </remarks>
[ApiController]
[Route("og")]
public sealed class OgImageController : ControllerBase
{
    private readonly OgImageService _ogImageService;
    private readonly MarkdownRenderingService _markdownRenderingService;
    private readonly BlogPostService _blogPostService;
    private readonly DevChallengeService _devChallengeService;
    private readonly NoteService _noteService;
    private readonly TutorialService _tutorialService;
    private readonly ProjectService _projectService;
    private readonly CreationService _creationService;

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
        return ServeCached("site", "site", null, () => _ogImageService.RenderFlatCard("BOOTH.DEV", Strings.MyName, Strings.Tagline));
    }

    /// <summary>
    ///     Gets the card for a blog post.
    /// </summary>
    [HttpGet("blog/{id:guid}.png")]
    public IActionResult GetBlogCard(Guid id)
    {
        var result = _blogPostService.GetPost(id, includeTrashed: true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        BlogPost post = result.Value;
        var description = _markdownRenderingService.RenderPlainTextExcerpt(post, out _);
        return ServeCached("blog", id.ToString("N"), post.UpdatedAt ?? post.PublishedAt,
            () => _ogImageService.RenderFlatCard("BLOG POST", post.Title, description));
    }

    /// <summary>
    ///     Gets the card for a tutorial article.
    /// </summary>
    [HttpGet("tutorial/{id:guid}.png")]
    public IActionResult GetTutorialCard(Guid id)
    {
        var result = _tutorialService.GetArticle(id, includeTrashed: true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        TutorialArticle article = result.Value;
        var description = _markdownRenderingService.RenderPlainTextExcerpt(article, out _);
        return ServeCached("tutorial", id.ToString("N"), article.UpdatedAt ?? article.PublishedAt,
            () => _ogImageService.RenderFlatCard("TUTORIAL", article.Title, description));
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

        var result = _devChallengeService.GetChallengeById(challengeId, includeTrashed: true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        DevChallenge challenge = result.Value;
        var description = _markdownRenderingService.RenderPlainTextPreview(challenge.Description);
        return ServeCached("challenge", ((Guid)challenge.Id).ToString("N"), challenge.UpdatedAt ?? challenge.PublishedAt,
            () => _ogImageService.RenderFlatCard("CHALLENGE", challenge.Title, description));
    }

    /// <summary>
    ///     Gets the card for a note.
    /// </summary>
    [HttpGet("note/{id:guid}.png")]
    public IActionResult GetNoteCard(Guid id)
    {
        var result = _noteService.GetNoteById(id, includeTrashed: true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        Note note = result.Value;
        var description = _markdownRenderingService.RenderPlainTextPreview(note.Content);
        return ServeCached("note", id.ToString("N"), note.UpdatedAt ?? note.PublishedAt,
            () => _ogImageService.RenderFlatCard("NOTE", note.Title, description));
    }

    /// <summary>
    ///     Gets the card for a project devlog entry.
    /// </summary>
    [HttpGet("devlog/{id:guid}.png")]
    public IActionResult GetDevlogCard(Guid id)
    {
        var result = _projectService.GetDevlogById(id, includeTrashed: true);
        if (result.IsFailed)
        {
            return NotFound();
        }

        ProjectDevlog devlog = result.Value;
        var description = _markdownRenderingService.RenderPlainTextPreview(devlog.Body);
        return ServeCached("devlog", id.ToString("N"), devlog.UpdatedAt ?? devlog.PublishedAt,
            () => _ogImageService.RenderFlatCard("DEVLOG", devlog.Title, description));
    }

    /// <summary>
    ///     Gets the card for a project, using its hero image as a backdrop when it has one.
    /// </summary>
    [HttpGet("project/{id:guid}.png")]
    public IActionResult GetProjectCard(Guid id)
    {
        var result = _projectService.GetProject(id);
        if (result.IsFailed)
        {
            return NotFound();
        }

        Project project = result.Value;
        var description = _markdownRenderingService.RenderPlainTextPreview(project.Description);

        return ServeCached("project", id.ToString("N"), null, () =>
        {
            var backdropPath = ResolveImagePath("projects", project.HeroUrl, project.CreatedAt, project.Id);
            return backdropPath is null
                ? _ogImageService.RenderFlatCard("PROJECT", project.Name, description)
                : _ogImageService.RenderPhotoCard("PROJECT", project.Name, description, backdropPath);
        });
    }

    /// <summary>
    ///     Gets the card for an artwork item, using its file as a backdrop.
    /// </summary>
    [HttpGet("artwork/{id:guid}.png")]
    public IActionResult GetArtworkCard(Guid id)
    {
        var result = _creationService.GetArtworkItem(id, includeTrashed: true);
        return GetCreationCard(result.IsFailed ? null : result.Value, "artwork");
    }

    /// <summary>
    ///     Gets the card for a music item. Music files are never images, so this always renders the flat card.
    /// </summary>
    [HttpGet("music/{id:guid}.png")]
    public IActionResult GetMusicCard(Guid id)
    {
        var result = _creationService.GetMusicItem(id, includeTrashed: true);
        return GetCreationCard(result.IsFailed ? null : result.Value, "music");
    }

    private IActionResult GetCreationCard(CreativeItem? item, string type)
    {
        if (item is null)
        {
            return NotFound();
        }

        var description = string.IsNullOrWhiteSpace(item.Description)
            ? null
            : _markdownRenderingService.RenderPlainTextPreview(item.Description);

        return ServeCached(type, item.Id.ToString("N"), null, () =>
        {
            var backdropPath = ResolveImagePath("content", item.FileName, item.PublishedAt, item.Id);
            return backdropPath is null
                ? _ogImageService.RenderFlatCard(type.ToUpperInvariant(), item.Title, description)
                : _ogImageService.RenderPhotoCard(type.ToUpperInvariant(), item.Title, description, backdropPath);
        });
    }

    /// <summary>
    ///     Resolves the physical path of a media file, but only if it actually resolves to an image - a photo card
    ///     backdrop is meaningless for e.g. an MP3.
    /// </summary>
    private static string? ResolveImagePath(string area, string? filename, DateTimeOffset published, Guid id)
    {
        if (string.IsNullOrEmpty(filename))
        {
            return null;
        }

        MediaKind kind = CdnMediaResolver.ResolveMediaKind(filename);
        if (kind != MediaKind.Image)
        {
            return null;
        }

        var path = CdnPaths.GetMediaPath(area, kind, published, id, filename);
        return System.IO.File.Exists(path) ? path : null;
    }

    /// <summary>
    ///     Serves a cached card if one exists and is still fresh, otherwise renders, caches, and serves a fresh one.
    /// </summary>
    /// <param name="type">The content type, used as the cache sub-directory.</param>
    /// <param name="key">The content's own ID, used as the cache filename.</param>
    /// <param name="contentUpdatedAt">
    ///     The content's last-modified timestamp, or <see langword="null" /> for content with no such concept - the
    ///     cached file is then treated as permanently fresh once it exists.
    /// </param>
    /// <param name="render">Renders a fresh card, only invoked on a cache miss.</param>
    private IActionResult ServeCached(string type, string key, DateTimeOffset? contentUpdatedAt, Func<byte[]> render)
    {
        var cachePath = Path.Combine(CdnPaths.GetRoot(), "og", OgImageService.TemplateVersion, type, $"{key}.png");
        var isFresh = System.IO.File.Exists(cachePath) &&
                      (contentUpdatedAt is null || System.IO.File.GetLastWriteTimeUtc(cachePath) >= contentUpdatedAt.Value.UtcDateTime);

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
