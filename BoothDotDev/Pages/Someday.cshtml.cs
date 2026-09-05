using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

/// <summary>
///     Represents the model for the someday page.
/// </summary>
public sealed class Someday : PageModel
{
    private readonly SomedayEntryService _somedayEntryService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Someday" /> class.
    /// </summary>
    /// <param name="somedayEntryService">The <see cref="SomedayEntryService" />.</param>
    public Someday(SomedayEntryService somedayEntryService)
    {
        _somedayEntryService = somedayEntryService;
    }

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
    public void OnGet()
    {
        Entries = _somedayEntryService.GetPublishedEntries();
    }
}
