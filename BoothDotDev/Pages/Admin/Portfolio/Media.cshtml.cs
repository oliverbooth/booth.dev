using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Portfolio;

/// <summary>
///     Represents the page model that receives the admin media manager's posts, for the files of a project or creation.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class MediaHandler : OwnerPostPage
{
    /// <summary>
    ///     The key the media manager reads its errors from, in <see cref="PageModel.TempData" />.
    /// </summary>
    public const string MediaErrorsKey = "MediaErrors";

    private readonly MediaService _mediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaHandler" /> class.
    /// </summary>
    /// <param name="mediaService">The <see cref="MediaService" />.</param>
    public MediaHandler(MediaService mediaService)
    {
        _mediaService = mediaService;
    }

    /// <inheritdoc />
    protected override string ErrorsKey
    {
        get => MediaErrorsKey;
    }

    /// <inheritdoc />
    protected override string SectionId
    {
        get => "media";
    }

    /// <summary>
    ///     Handles the POST request for uploading files.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="files">The uploaded files.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public async Task<IActionResult> OnPostUploadAsync(
        Guid ownerId,
        List<IFormFile> files,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (_mediaService.FindOwner(ownerId) is not { } owner)
        {
            return NotFound();
        }

        List<string> errors = [];
        foreach (var file in files)
        {
            var result = await _mediaService.AddAsync(owner, file, cancellationToken);
            if (result.IsFailed)
            {
                errors.AddRange(result.Errors.Select(e => e.Message));
            }
        }

        return Back(returnUrl, errors);
    }

    /// <summary>
    ///     Handles the POST request for deleting a file.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="mediaId">The ID of the file.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRemove(Guid ownerId, Guid mediaId, string? returnUrl)
    {
        return _mediaService.FindOwner(ownerId) is { } owner
            ? Back(returnUrl, _mediaService.Remove(owner, mediaId))
            : NotFound();
    }

    /// <summary>
    ///     Handles the POST request for making a file the cover.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="mediaId">The ID of the file.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostCover(Guid ownerId, Guid mediaId, string? returnUrl)
    {
        return Back(returnUrl, _mediaService.SetCover(ownerId, mediaId));
    }

    /// <summary>
    ///     Handles the POST request for clearing the cover.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostClearCover(Guid ownerId, string? returnUrl)
    {
        return Back(returnUrl, _mediaService.ClearCover(ownerId));
    }

    /// <summary>
    ///     Handles the POST request for saving the order of the files.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="ids">The IDs of the files, in their new order.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostReorder(Guid ownerId, List<Guid> ids, string? returnUrl)
    {
        return Back(returnUrl, _mediaService.Reorder(ownerId, ids));
    }

    /// <summary>
    ///     Handles the POST request for setting the alternative text of a file.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="mediaId">The ID of the file.</param>
    /// <param name="alt">The alternative text.</param>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostAlt(Guid ownerId, Guid mediaId, string? alt, string? returnUrl)
    {
        return Back(returnUrl, _mediaService.SetAlt(ownerId, mediaId, alt));
    }
}
