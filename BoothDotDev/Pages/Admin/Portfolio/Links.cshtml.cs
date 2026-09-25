using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Portfolio;

/// <summary>
///     Represents the page model that receives the admin link manager's posts, for the links of a project or creation.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class LinksHandler : OwnerPostPage
{
    /// <summary>
    ///     The key the link manager reads its errors from, in <see cref="PageModel.TempData" />.
    /// </summary>
    public const string LinkErrorsKey = "LinkErrors";

    private readonly LinkService _linkService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LinksHandler" /> class.
    /// </summary>
    /// <param name="linkService">The <see cref="LinkService" />.</param>
    public LinksHandler(LinkService linkService)
    {
        _linkService = linkService;
    }

    /// <inheritdoc />
    protected override string ErrorsKey
    {
        get => LinkErrorsKey;
    }

    /// <inheritdoc />
    protected override string SectionId
    {
        get => "links";
    }

    /// <summary>
    ///     Handles the POST request for adding a link.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="kind">The kind of place the link leads to.</param>
    /// <param name="url">The address the link leads to.</param>
    /// <param name="label">The text to show in place of the default for the kind.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostAdd(Guid ownerId, LinkKind kind, string url, string? label, string? returnUrl)
    {
        return Back(returnUrl, _linkService.Add(ownerId, kind, url, label));
    }

    /// <summary>
    ///     Handles the POST request for changing a link.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="linkId">The ID of the link.</param>
    /// <param name="kind">The kind of place the link leads to.</param>
    /// <param name="url">The address the link leads to.</param>
    /// <param name="label">The text to show in place of the default for the kind.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostUpdate(Guid ownerId, Guid linkId, LinkKind kind, string url, string? label, string? returnUrl)
    {
        return Back(returnUrl, _linkService.Update(ownerId, linkId, kind, url, label));
    }

    /// <summary>
    ///     Handles the POST request for removing a link.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="linkId">The ID of the link.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRemove(Guid ownerId, Guid linkId, string? returnUrl)
    {
        return Back(returnUrl, _linkService.Remove(ownerId, linkId));
    }

    /// <summary>
    ///     Handles the POST request for making a link the primary one.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="linkId">The ID of the link.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPrimary(Guid ownerId, Guid linkId, string? returnUrl)
    {
        return Back(returnUrl, _linkService.SetPrimary(ownerId, linkId));
    }

    /// <summary>
    ///     Handles the POST request for clearing the primary link.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostClearPrimary(Guid ownerId, string? returnUrl)
    {
        return Back(returnUrl, _linkService.ClearPrimary(ownerId));
    }

    /// <summary>
    ///     Handles the POST request for saving the order of the links.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="ids">The IDs of the links, in their new order.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostReorder(Guid ownerId, List<Guid> ids, string? returnUrl)
    {
        return Back(returnUrl, _linkService.Reorder(ownerId, ids));
    }
}
