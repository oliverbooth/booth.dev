using BoothDotDev.Data.Models;
using BoothDotDev.Services;

namespace BoothDotDev.Pages.Admin.Someday;

using SomedayEntry = SomedayEntry;

/// <summary>
///     Represents the page model for the admin someday trash page.
/// </summary>
public sealed class Trash : TrashPageModel<SomedayEntry>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="somedayEntryService">The <see cref="SomedayEntryService" />.</param>
    public Trash(SomedayEntryService somedayEntryService)
        : base(somedayEntryService.GetTrashedEntries, id => somedayEntryService.RestoreEntry(id), id => somedayEntryService.PermanentlyDeleteEntry(id))
    {
    }

    /// <summary>
    ///     Gets the list of trashed someday entries, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed entries.</value>
    public IReadOnlyList<SomedayEntry> Entries => Items;
}
