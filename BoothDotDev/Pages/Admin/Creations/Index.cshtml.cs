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
    ///     Gets the list of creations, newest first.
    /// </summary>
    /// <value>The list of creations.</value>
    public IReadOnlyList<CreationListItem> Creations { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Creations =
        [
            .. _creationService.GetAllCreations()
                .Select(c => new CreationListItem(c.Id, c.Title, c.Slug, c.Kind, c.Visibility, c.PublishedAt))
        ];
    }

    /// <summary>
    ///     Handles the POST request for moving a creation to the trash.
    /// </summary>
    /// <param name="id">The ID of the creation to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        _creationService.TrashCreation(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Represents a single row in the creations list.
    /// </summary>
    /// <param name="Id">The ID of the creation.</param>
    /// <param name="Title">The title of the creation.</param>
    /// <param name="Slug">The slug of the creation.</param>
    /// <param name="Kind">The kind of the creation.</param>
    /// <param name="Visibility">The visibility of the creation.</param>
    /// <param name="PublishedAt">The publication date and time of the creation.</param>
    public sealed record CreationListItem(
        Guid Id,
        string Title,
        string Slug,
        CreationKind Kind,
        Visibility Visibility,
        DateTimeOffset PublishedAt);
}
