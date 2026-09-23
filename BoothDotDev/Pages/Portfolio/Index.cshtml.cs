using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Portfolio;

/// <summary>
///     Represents the page model for the "stuff i made" page - a merged view of Projects and Create.
/// </summary>
public sealed class Made : PageModel
{
    private readonly CreationService _creationService;
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Made" /> class.
    /// </summary>
    /// <param name="projectService">The project service.</param>
    /// <param name="creationService">The creation service.</param>
    public Made(ProjectService projectService, CreationService creationService)
    {
        _projectService = projectService;
        _creationService = creationService;
    }

    /// <summary>
    ///     Gets the artwork items.
    /// </summary>
    /// <value>The artwork items.</value>
    public IReadOnlyList<ArtworkItem> ArtworkItems { get; private set; } = [];

    /// <summary>
    ///     Gets the music items.
    /// </summary>
    /// <value>The music items.</value>
    public IReadOnlyList<MusicItem> MusicItems { get; private set; } = [];

    /// <summary>
    ///     Gets every project, across every status.
    /// </summary>
    /// <value>Every project.</value>
    public IReadOnlyList<Data.Models.Project> Projects { get; private set; } = [];

    /// <summary>
    ///     Handles the HTTP GET request.
    /// </summary>
    public void OnGet()
    {
        Projects =
        [
            .._projectService.GetProjects(),
            .._projectService.GetProjects(ProjectStatus.Past),
            .._projectService.GetProjects(ProjectStatus.Retired),
            .._projectService.GetProjects(ProjectStatus.Hiatus)
        ];
        ArtworkItems = _creationService.GetArtworkItems();
        MusicItems = _creationService.GetMusicItems();
    }
}
