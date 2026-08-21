using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using BoothDotDev.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin;

/// <summary>
///     Represents the login page model for the admin section of the application.
/// </summary>
public sealed class Login : PageModel
{
    private readonly UserService _userService;
    private readonly IDataProtector _protector;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Login" /> class.
    /// </summary>
    /// <param name="userService">The <see cref="UserService" />.</param>
    /// <param name="provider">The <see cref="IDataProtectionProvider" /> used to create a data protector.</param>
    public Login(UserService userService, IDataProtectionProvider provider)
    {
        _userService = userService;
        _protector = provider.CreateProtector("AdminLogin.PendingTotp");
    }

    /// <summary>
    ///     Gets or sets the input model for the login form, containing the email and password fields.
    /// </summary>
    /// <value>The input model for the login form.</value>
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    /// <summary>
    ///     Handles the POST request.
    /// </summary>
    /// <returns>The <see cref="IActionResult" /> representing the result of the POST request.</returns>
    public IActionResult OnPost()
    {
        var result = _userService.VerifyPassword(Input.Email, Input.Password);

        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
            return Page();
        }

        var user = result.Value;
        if (string.IsNullOrEmpty(user.TotpSecret))
        {
            ModelState.AddModelError(string.Empty, "TOTP is not configured for this account. Contact the site administrator.");
            return Page();
        }

        var payload = _protector.Protect($"{user.Id}|{DateTimeOffset.UtcNow:O}");

        Response.Cookies.Append("pending_totp", payload,
            new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, MaxAge = TimeSpan.FromMinutes(5)
            });

        return RedirectToPage("/Admin/LoginTotp");
    }

    private bool TryGetPendingUserId(out Guid userId)
    {
        userId = Guid.Empty;

        if (!Request.Cookies.TryGetValue("pending_totp", out var cookie))
        {
            return false;
        }

        try
        {
            var payload = _protector.Unprotect(cookie);
            var parts = payload.Split('|');
            var issuedAt = DateTimeOffset.Parse(parts[1]);

            if (DateTimeOffset.UtcNow - issuedAt > TimeSpan.FromMinutes(5))
            {
                return false;
            }

            return Guid.TryParse(parts[0], out userId);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Represents the input model for the login form, containing the email and password fields.
    /// </summary>
    public sealed class LoginInput
    {
        /// <summary>
        ///     Gets or sets the email address of the user attempting to log in.
        /// </summary>
        /// <value>The email address of the user attempting to log in.</value>
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the password of the user attempting to log in.
        /// </summary>
        /// <value>The password of the user attempting to log in.</value>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
