using BoothDotDev.Data;
using BoothDotDev.Services;
using DEDrake;
using Microsoft.AspNetCore.Mvc;

namespace BoothDotDev.Controllers;

/// <summary>
///     Represents the command palette controller, serving the navigable index behind the site's Ctrl+K shortcut.
/// </summary>
[ApiController]
[Route("api/command-palette")]
[Produces("application/json")]
public sealed class CommandPaletteController : ControllerBase
{
    private readonly BlogPostService _blogPostService;
    private readonly DevChallengeService _devChallengeService;
    private readonly NoteService _noteService;
    private readonly ProjectService _projectService;
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandPaletteController" /> class.
    /// </summary>
    public CommandPaletteController(BlogPostService blogPostService,
        DevChallengeService devChallengeService,
        NoteService noteService,
        ProjectService projectService,
        TutorialService tutorialService)
    {
        _blogPostService = blogPostService;
        _devChallengeService = devChallengeService;
        _noteService = noteService;
        _projectService = projectService;
        _tutorialService = tutorialService;
    }

    /// <summary>
    ///     Gets the full command palette index: the site's static pages plus every piece of content visible to the
    ///     current caller. Signed-in visitors also see private/unlisted content, matching what they'd see browsing
    ///     the site directly.
    /// </summary>
    /// <returns>The command palette entries.</returns>
    [HttpGet("index")]
    public IActionResult GetIndex()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var visibility = isAuthenticated ? Visibility.None : Visibility.Published;

        List<CommandPaletteEntry> entries = [.. StaticPages(isAuthenticated)];

        entries.AddRange(_blogPostService.GetAllBlogPosts(visibility: visibility)
            .Select(post => new CommandPaletteEntry(post.Title, $"/blog/{post.Slug}", "Blog")));

        entries.AddRange(_noteService.GetAllNotes(visibility)
            .Select(note => new CommandPaletteEntry(note.Title, $"/note/{(ShortGuid)note.Id}", "Notes")));

        entries.AddRange(_devChallengeService.GetAllChallenges(visibility)
            .Select(challenge => new CommandPaletteEntry(challenge.Title, $"/challenge/{challenge.Id}", "Challenges")));

        entries.AddRange(_tutorialService.GetAllArticles(visibility)
            .Select(article => new CommandPaletteEntry(article.Title, $"/learn/{_tutorialService.GetFullSlug(article)}", "Learn")));

        foreach (var project in _projectService.GetAllProjects())
        {
            entries.Add(new CommandPaletteEntry(project.Name, $"/project/{project.Slug}", "Projects"));
            entries.AddRange(_projectService.GetDevlogs(project, visibility)
                .Select(devlog => new CommandPaletteEntry(devlog.Title, $"/project/{project.Slug}/devlog/{devlog.Slug}", "Projects")));
        }

        return Ok(entries);
    }

    /// <summary>
    ///     Returns the site's fixed top-level pages. Unlike content, these aren't backed by a service, so they're
    ///     just listed here directly.
    /// </summary>
    /// <param name="isAuthenticated">Whether the current caller is signed in.</param>
    private static IEnumerable<CommandPaletteEntry> StaticPages(bool isAuthenticated)
    {
        yield return new CommandPaletteEntry("Home", "/", "Pages");
        yield return new CommandPaletteEntry("Write", "/blog", "Pages");
        yield return new CommandPaletteEntry("Build", "/projects", "Pages");
        yield return new CommandPaletteEntry("Learn", "/learn", "Pages");
        yield return new CommandPaletteEntry("Create", "/create", "Pages");
        yield return new CommandPaletteEntry("Now", "/now", "Pages");
        yield return new CommandPaletteEntry("Someday", "/someday", "Pages");
        yield return new CommandPaletteEntry("Books", "/books", "Pages");
        yield return new CommandPaletteEntry("Watchlist", "/watchlist", "Pages");
        yield return new CommandPaletteEntry("About", "/about", "Pages");
        yield return new CommandPaletteEntry("Donate", "/donate", "Pages");

        if (isAuthenticated)
        {
            yield return new CommandPaletteEntry("Admin", "/admin", "Pages");
        }
    }
}

/// <summary>
///     Represents a single navigable entry in the command palette.
/// </summary>
/// <param name="Title">The entry's display title.</param>
/// <param name="Url">The URL the entry navigates to.</param>
/// <param name="Section">The group the entry is displayed under.</param>
public sealed record CommandPaletteEntry(string Title, string Url, string Section);
