using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin;

/// <summary>
///     Represents the page model for signing out of the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Logout : PageModel
{
    /// <summary>
    ///     Handles the POST request.
    /// </summary>
    /// <value>The <see cref="IActionResult" /> representing the result of the POST request.</value>
    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Admin/Login");
    }
}
