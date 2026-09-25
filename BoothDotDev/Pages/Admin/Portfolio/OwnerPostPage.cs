using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Portfolio;

/// <summary>
///     Represents the base of a page model that only receives posts from a manager on the edit page of a project or
///     creation, then sends the browser back to it.
/// </summary>
public abstract class OwnerPostPage : PageModel
{
    /// <summary>
    ///     Gets the key the manager reads its errors from, in <see cref="PageModel.TempData" />.
    /// </summary>
    /// <value>The key.</value>
    protected abstract string ErrorsKey { get; }

    /// <summary>
    ///     Gets the ID of the section of the edit page the manager is in, so the browser returns to it.
    /// </summary>
    /// <value>The ID of the section.</value>
    protected abstract string SectionId { get; }

    /// <summary>
    ///     Handles the GET request. There is nothing to show.
    /// </summary>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet()
    {
        return NotFound();
    }

    /// <summary>
    ///     Sends the browser back to where the post came from, keeping the errors of <paramref name="result" /> to show there.
    /// </summary>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <param name="result">The result of the change that was made.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    protected IActionResult Back(string? returnUrl, ResultBase result)
    {
        return Back(returnUrl, result.Errors.Select(e => e.Message).ToList());
    }

    /// <summary>
    ///     Sends the browser back to where the post came from, keeping <paramref name="errors" /> to show there.
    /// </summary>
    /// <param name="returnUrl">The local URL to return to.</param>
    /// <param name="errors">The problems to show, if any.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    protected IActionResult Back(string? returnUrl, List<string> errors)
    {
        if (errors.Count > 0)
        {
            TempData[ErrorsKey] = string.Join(" ", errors);
        }

        return returnUrl is not null && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect($"{returnUrl}#{SectionId}")
            : RedirectToPage("/Admin/Portfolio/Index");
    }
}
