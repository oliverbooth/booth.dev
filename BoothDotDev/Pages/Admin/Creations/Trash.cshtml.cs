using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Creations;

/// <summary>
///     Represents the page model for the admin creations trash page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Trash : PageModel
{
    private readonly CreationService _creationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="creationService">The <see cref="CreationService" />.</param>
    public Trash(CreationService creationService)
    {
        _creationService = creationService;
    }

    /// <summary>
    ///     Gets the combined list of trashed artwork and music items, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed creations.</value>
    public IReadOnlyList<TrashedCreationListItem> Creations { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        var artwork = _creationService.GetTrashedArtworkItems()
            .Select(a => new TrashedCreationListItem(a.Id, a.Title, "Artwork", a.Visibility, a.TrashedAt!.Value));
        var music = _creationService.GetTrashedMusicItems()
            .Select(m => new TrashedCreationListItem(m.Id, m.Title, "Music", m.Visibility, m.TrashedAt!.Value));

        Creations = [.. artwork.Concat(music).OrderByDescending(c => c.TrashedAt)];
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed creation.
    /// </summary>
    /// <param name="id">The ID of the creation to restore.</param>
    /// <param name="type">The type of the creation, either <c>"artwork"</c> or <c>"music"</c>.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id, string type)
    {
        if (string.Equals(type, "music", StringComparison.OrdinalIgnoreCase))
        {
            _creationService.RestoreMusicItem(id);
        }
        else
        {
            _creationService.RestoreArtworkItem(id);
        }

        return RedirectToPage();
    }

    /// <summary>
    ///     Represents a single row in the combined trashed creations list.
    /// </summary>
    /// <param name="Id">The ID of the creation.</param>
    /// <param name="Title">The title of the creation.</param>
    /// <param name="Type">The type of the creation, either <c>"Artwork"</c> or <c>"Music"</c>.</param>
    /// <param name="Visibility">The visibility of the creation.</param>
    /// <param name="TrashedAt">The date and time the creation was trashed.</param>
    public sealed record TrashedCreationListItem(Guid Id, string Title, string Type, Visibility Visibility, DateTimeOffset TrashedAt);
}
