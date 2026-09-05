using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Someday;

using SomedayEntry = SomedayEntry;

/// <summary>
///     Represents the page model for the admin someday page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly SomedayEntryService _somedayEntryService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="somedayEntryService">The <see cref="SomedayEntryService" />.</param>
    public Index(SomedayEntryService somedayEntryService)
    {
        _somedayEntryService = somedayEntryService;
    }

    /// <summary>
    ///     Gets the list of someday entries, in their curated display order.
    /// </summary>
    /// <value>The list of someday entries.</value>
    public IReadOnlyList<SomedayEntry> Entries { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Entries = _somedayEntryService.GetAllEntries();
    }

    /// <summary>
    ///     Handles the POST request for moving an entry to the trash.
    /// </summary>
    /// <param name="id">The ID of the entry to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        _somedayEntryService.TrashEntry(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for saving a new display order for every entry.
    /// </summary>
    /// <param name="ids">Every entry's ID, in its new display order.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostReorder(List<Guid> ids)
    {
        _somedayEntryService.Reorder(ids);
        return RedirectToPage();
    }
}
