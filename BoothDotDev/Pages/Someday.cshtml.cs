using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

/// <summary>
///     Represents the model for the someday page.
/// </summary>
public sealed class Someday : PageModel
{
    private readonly IAuthorizationService _authorizationService;
    private readonly SomedayEntryService _somedayEntryService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Someday" /> class.
    /// </summary>
    /// <param name="somedayEntryService">The <see cref="SomedayEntryService" />.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService" />.</param>
    public Someday(SomedayEntryService somedayEntryService, IAuthorizationService authorizationService)
    {
        _somedayEntryService = somedayEntryService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    ///     Gets a value indicating whether the visitor can mark entries as achieved.
    /// </summary>
    /// <value><see langword="true" /> if the visitor is an admin; otherwise, <see langword="false" />.</value>
    public bool CanMarkAchieved { get; private set; }

    /// <summary>
    ///     Gets every published, non-trashed someday entry, in curated display order.
    /// </summary>
    /// <value>The entries to render on the page.</value>
    public IReadOnlyList<SomedayEntry> Entries { get; private set; } = [];

    /// <summary>
    ///     Gets the date and time the page was last updated, i.e. the most recent update across every entry.
    /// </summary>
    /// <value>The most recent update date and time, or <see langword="null" /> if there are no entries yet.</value>
    public DateTimeOffset? UpdatedAt
    {
        get => Entries.Count == 0 ? null : Entries.Max(e => e.UpdatedAt ?? e.PublishedAt);
    }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public async Task OnGetAsync()
    {
        Entries = _somedayEntryService.GetPublishedEntries();
        CanMarkAchieved = await IsAdminAsync();
    }

    /// <summary>
    ///     Handles the POST request for marking an entry as achieved, or clearing that.
    /// </summary>
    /// <param name="id">The ID of the entry to toggle.</param>
    /// <returns>A redirect back to the entry on the page, or an error result.</returns>
    public async Task<IActionResult> OnPostToggleAchievedAsync(Guid id)
    {
        if (!await IsAdminAsync())
        {
            return Forbid();
        }

        var result = _somedayEntryService.ToggleAchieved(id);
        if (result.IsFailed)
        {
            return NotFound();
        }

        return RedirectToPage(pageName: null, pageHandler: null, routeValues: null, fragment: result.Value.Slug);
    }

    private async Task<bool> IsAdminAsync()
    {
        return (await _authorizationService.AuthorizeAsync(User, "Admin")).Succeeded;
    }
}
