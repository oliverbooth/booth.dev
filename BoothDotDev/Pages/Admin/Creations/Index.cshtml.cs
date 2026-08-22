using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Creations;

/// <summary>
///     Represents the page model for the admin creations page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly CreationService _creationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="creationService">The <see cref="CreationService" />.</param>
    public Index(CreationService creationService)
    {
        _creationService = creationService;
    }

    /// <summary>
    ///     Gets the combined list of artwork and music items, newest first.
    /// </summary>
    /// <value>The list of creations.</value>
    public IReadOnlyList<CreationListItem> Creations { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        var artwork = _creationService.GetAllArtworkItems()
            .Select(a => new CreationListItem(a.Id, a.Title, "Artwork", a.Visibility, a.PublishedAt));
        var music = _creationService.GetAllMusicItems()
            .Select(m => new CreationListItem(m.Id, m.Title, "Music", m.Visibility, m.PublishedAt));

        Creations = [.. artwork.Concat(music).OrderByDescending(c => c.PublishedAt)];
    }

    /// <summary>
    ///     Handles the POST request for moving a creation to the trash.
    /// </summary>
    /// <param name="id">The ID of the creation to trash.</param>
    /// <param name="type">The type of the creation, either <c>"artwork"</c> or <c>"music"</c>.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id, string type)
    {
        if (string.Equals(type, "music", StringComparison.OrdinalIgnoreCase))
        {
            _creationService.TrashMusicItem(id);
        }
        else
        {
            _creationService.TrashArtworkItem(id);
        }

        return RedirectToPage();
    }

    /// <summary>
    ///     Represents a single row in the combined creations list.
    /// </summary>
    /// <param name="Id">The ID of the creation.</param>
    /// <param name="Title">The title of the creation.</param>
    /// <param name="Type">The type of the creation, either <c>"Artwork"</c> or <c>"Music"</c>.</param>
    /// <param name="Visibility">The visibility of the creation.</param>
    /// <param name="Published">The publication date and time of the creation.</param>
    public sealed record CreationListItem(Guid Id, string Title, string Type, Visibility Visibility, DateTimeOffset PublishedAt);
}
