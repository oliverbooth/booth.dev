using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Someday;

using SomedayEntry = SomedayEntry;

/// <summary>
///     Represents the page model for the admin someday trash page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Trash : PageModel
{
    private readonly SomedayEntryService _somedayEntryService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="somedayEntryService">The <see cref="SomedayEntryService" />.</param>
    public Trash(SomedayEntryService somedayEntryService)
    {
        _somedayEntryService = somedayEntryService;
    }

    /// <summary>
    ///     Gets the list of trashed someday entries, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed entries.</value>
    public IReadOnlyList<SomedayEntry> Entries { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Entries = _somedayEntryService.GetTrashedEntries();
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed entry.
    /// </summary>
    /// <param name="id">The ID of the entry to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        _somedayEntryService.RestoreEntry(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting a single trashed entry.
    /// </summary>
    /// <param name="id">The ID of the entry to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDelete(Guid id)
    {
        _somedayEntryService.PermanentlyDeleteEntry(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting every selected trashed entry.
    /// </summary>
    /// <param name="ids">The IDs of the entries to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDeleteBulk(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            _somedayEntryService.PermanentlyDeleteEntry(id);
        }

        return RedirectToPage();
    }
}
