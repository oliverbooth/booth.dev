using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin;

/// <summary>
///     Provides a reusable base for admin "trash" list pages.
/// </summary>
/// <typeparam name="TItem">The type of item shown in the trash list.</typeparam>
[Authorize(Policy = "Admin")]
public abstract class TrashPageModel<TItem> : PageModel
{
    private readonly Func<IReadOnlyList<TItem>> _getTrashed;
    private readonly Action<Guid> _restore;
    private readonly Action<Guid> _permanentlyDelete;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TrashPageModel{TItem}" /> class.
    /// </summary>
    /// <param name="getTrashed">A delegate which retrieves the trashed items, newest-trashed first.</param>
    /// <param name="restore">A delegate which restores a single trashed item by ID.</param>
    /// <param name="permanentlyDelete">A delegate which permanently deletes a single trashed item by ID.</param>
    protected TrashPageModel(Func<IReadOnlyList<TItem>> getTrashed, Action<Guid> restore, Action<Guid> permanentlyDelete)
    {
        _getTrashed = getTrashed;
        _restore = restore;
        _permanentlyDelete = permanentlyDelete;
    }

    /// <summary>
    ///     Gets the list of trashed items, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed items.</value>
    protected IReadOnlyList<TItem> Items { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Items = _getTrashed();
    }

    /// <summary>
    ///     Handles the POST request for restoring a single trashed item.
    /// </summary>
    /// <param name="id">The ID of the item to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        _restore(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting a single trashed item.
    /// </summary>
    /// <param name="id">The ID of the item to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDelete(Guid id)
    {
        _permanentlyDelete(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting every selected trashed item.
    /// </summary>
    /// <param name="ids">The IDs of the items to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDeleteBulk(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            _permanentlyDelete(id);
        }

        return RedirectToPage();
    }
}
