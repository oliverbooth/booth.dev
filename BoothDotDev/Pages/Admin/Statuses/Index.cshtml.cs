using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Statuses;

/// <summary>
///     Represents the page model for the admin statuses page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly StatusService _statusService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="statusService">The status service.</param>
    public Index(StatusService statusService)
    {
        _statusService = statusService;
    }

    /// <summary>
    ///     Gets the statuses in rotation, newest first.
    /// </summary>
    /// <value>The statuses in rotation.</value>
    public IReadOnlyList<Status> Active { get; private set; } = [];

    /// <summary>
    ///     Gets the statuses out of rotation, newest first.
    /// </summary>
    /// <value>The statuses out of rotation.</value>
    public IReadOnlyList<Status> Inactive { get; private set; } = [];

    /// <summary>
    ///     Gets or sets the problem with the last change, if any.
    /// </summary>
    /// <value>The problem, or <see langword="null" /> if the last change worked.</value>
    [TempData]
    public string? Errors { get; set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        var statuses = _statusService.GetAll();
        Active = [.. statuses.Where(s => s.IsActive)];
        Inactive = [.. statuses.Where(s => !s.IsActive)];
    }

    /// <summary>
    ///     Handles the POST request for adding a status.
    /// </summary>
    /// <param name="text">The text of the status.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostAdd(string? text)
    {
        return Back(_statusService.Add(text ?? string.Empty));
    }

    /// <summary>
    ///     Handles the POST request for changing the text of a status.
    /// </summary>
    /// <param name="id">The ID of the status.</param>
    /// <param name="text">The new text.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid id, string? text)
    {
        return Back(_statusService.Update(id, text ?? string.Empty));
    }

    /// <summary>
    ///     Handles the POST request for putting a status in or out of rotation.
    /// </summary>
    /// <param name="id">The ID of the status.</param>
    /// <param name="isActive"><see langword="true" /> to put the status in rotation; otherwise, <see langword="false" />.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSetActive(Guid id, bool isActive)
    {
        return Back(_statusService.SetActive(id, isActive));
    }

    /// <summary>
    ///     Handles the POST request for making a status the only one in rotation.
    /// </summary>
    /// <param name="id">The ID of the status.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSetOnlyActive(Guid id)
    {
        return Back(_statusService.SetOnlyActive(id));
    }

    /// <summary>
    ///     Handles the POST request for removing a status.
    /// </summary>
    /// <param name="id">The ID of the status.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        return Back(_statusService.Remove(id));
    }

    private RedirectToPageResult Back(ResultBase result)
    {
        if (result.IsFailed)
        {
            Errors = string.Join(" ", result.Errors.Select(e => e.Message));
        }

        return RedirectToPage();
    }
}
