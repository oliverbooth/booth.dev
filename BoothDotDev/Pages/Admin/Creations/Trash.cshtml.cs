using BoothDotDev.Data;
using BoothDotDev.Services;

namespace BoothDotDev.Pages.Admin.Creations;

/// <summary>
///     Represents the page model for the admin creations trash page.
/// </summary>
public sealed class Trash : TrashPageModel<Trash.TrashedCreationListItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="creationService">The <see cref="CreationService" />.</param>
    public Trash(CreationService creationService)
        : base(
            () =>
            [
                .. creationService.GetTrashedCreations()
                    .Select(c => new TrashedCreationListItem(c.Id, c.Title, c.Kind, c.Visibility, c.TrashedAt!.Value))
            ],
            id => creationService.RestoreCreation(id),
            id => creationService.PermanentlyDeleteCreation(id))
    {
    }

    /// <summary>
    ///     Gets the list of trashed creations, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed creations.</value>
    public IReadOnlyList<TrashedCreationListItem> Creations => Items;

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
