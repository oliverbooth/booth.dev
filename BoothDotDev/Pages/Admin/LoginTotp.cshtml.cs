using System.Security.Claims;
using System.Security.Cryptography;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin;

/// <summary>
///     Represents the login page model for the admin section of the application that handles TOTP verification.
/// </summary>
public sealed class LoginTotp : PageModel
{
    private readonly IDataProtector _protector;
    private readonly UserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginTotp" /> class.
    /// </summary>
    /// <param name="userService">The <see cref="UserService" />.</param>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    public LoginTotp(UserService userService, IDataProtectionProvider dataProtectionProvider)
    {
        _userService = userService;
        _protector = dataProtectionProvider.CreateProtector("AdminLogin.PendingTotp");
    }

    /// <summary>
    ///     Gets or sets the TOTP code entered by the user for verification.
    /// </summary>
    /// <value>The TOTP code entered by the user for verification.</value>
    [BindProperty]
    public string TotpCode { get; set; } = string.Empty;

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <returns>The <see cref="IActionResult" /> representing the result of the GET request.</returns>
    public IActionResult OnGet()
    {
        var result = GetPendingUserId();
        if (result.IsFailed)
        {
            return RedirectToPage("/Admin/Login");
        }

        return Page();
    }

    /// <summary>
    ///     Handles the POST request.
    /// </summary>
    /// <returns>The <see cref="IActionResult" /> representing the result of the POST request.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        var pendingResult = GetPendingUserId();
        if (pendingResult.IsFailed)
        {
            return RedirectToPage("/Admin/Login");
        }

        var userId = pendingResult.Value;


        var result = _userService.VerifyTotp(userId, TotpCode);
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return Page();
        }

        Response.Cookies.Delete("pending_totp");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.GivenName, result.Value.DisplayName),
            new(ClaimTypes.Email, result.Value.EmailAddress)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
        return RedirectToPage("/Admin/Index");
    }

    private Result<Guid> GetPendingUserId()
    {
        if (!Request.Cookies.TryGetValue("pending_totp", out var cookie))
        {
            return Result.Fail("Pending TOTP cookie not found.");
        }

        try
        {
            var payload = _protector.Unprotect(cookie);
            var parts = payload.Split('|');
            var issuedAt = DateTimeOffset.Parse(parts[1]);

            if (DateTimeOffset.UtcNow - issuedAt > TimeSpan.FromMinutes(5))
            {
                return Result.Fail("Pending TOTP cookie has expired.");
            }

            if (!Guid.TryParse(parts[0], out var userId))
            {
                return Result.Fail("Invalid user ID in pending TOTP cookie.");
            }

            return Result.Ok(userId);
        }
        catch (CryptographicException exception)
        {
            return Result.Fail(exception.Message);
        }
    }
}
