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
    ///     Gets the list of trashed creations, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed creations.</value>
    public IReadOnlyList<TrashedCreationListItem> Creations { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Creations =
        [
            .._creationService.GetTrashedCreations()
                .Select(c => new TrashedCreationListItem(c.Id, c.Title, c.Kind, c.Visibility, c.TrashedAt!.Value))
        ];
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed creation.
    /// </summary>
    /// <param name="id">The ID of the creation to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        _creationService.RestoreCreation(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting a single trashed creation.
    /// </summary>
    /// <param name="id">The ID of the creation to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDelete(Guid id)
    {
        _creationService.PermanentlyDeleteCreation(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting every selected trashed creation.
    /// </summary>
    /// <param name="selections">The IDs of the creations to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDeleteBulk(List<Guid> selections)
    {
        foreach (var id in selections)
        {
            _creationService.PermanentlyDeleteCreation(id);
        }

        return RedirectToPage();
    }

    /// <summary>
    ///     Represents a single row in the trashed creations list.
    /// </summary>
    /// <param name="Id">The ID of the creation.</param>
    /// <param name="Title">The title of the creation.</param>
    /// <param name="Kind">The kind of the creation.</param>
    /// <param name="Visibility">The visibility of the creation.</param>
    /// <param name="TrashedAt">The date and time the creation was trashed.</param>
    public sealed record TrashedCreationListItem(
        Guid Id,
        string Title,
        CreationKind Kind,
        Visibility Visibility,
        DateTimeOffset TrashedAt);
}
