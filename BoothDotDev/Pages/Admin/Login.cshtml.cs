using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using BoothDotDev.Services;
using Fido2NetLib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin;

/// <summary>
///     Represents the login page model for the admin section of the application.
/// </summary>
public sealed class Login : PageModel
{
    private const string PasskeyLoginCookieName = "webauthn_login_challenge";
    private static readonly TimeSpan PasskeyChallengeLifetime = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly UserService _userService;
    private readonly PasskeyService _passkeyService;
    private readonly IDataProtector _protector;
    private readonly IDataProtector _passkeyProtector;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Login" /> class.
    /// </summary>
    /// <param name="userService">The <see cref="UserService" />.</param>
    /// <param name="passkeyService">The <see cref="PasskeyService" />.</param>
    /// <param name="provider">The <see cref="IDataProtectionProvider" /> used to create a data protector.</param>
    public Login(UserService userService, PasskeyService passkeyService, IDataProtectionProvider provider)
    {
        _userService = userService;
        _passkeyService = passkeyService;
        _protector = provider.CreateProtector("AdminLogin.PendingTotp");
        _passkeyProtector = provider.CreateProtector("AdminLogin.PasskeyLogin");
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

    /// <summary>
    ///     Handles the POST request for beginning a usernameless passkey login ceremony.
    /// </summary>
    /// <returns>The assertion options for the browser to pass to <c>navigator.credentials.get</c>, as raw JSON.</returns>
    public IActionResult OnPostBeginPasskeyLogin()
    {
        var options = _passkeyService.BeginLogin();
        var pending = new PendingPasskeyLogin(options.ToJson(), DateTimeOffset.UtcNow);
        var payload = _passkeyProtector.Protect(JsonSerializer.Serialize(pending));

        Response.Cookies.Append(PasskeyLoginCookieName, payload,
            new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, MaxAge = PasskeyChallengeLifetime
            });

        return Content(options.ToJson(), "application/json");
    }

    /// <summary>
    ///     Handles the POST request for completing a passkey login ceremony and signing the user in.
    /// </summary>
    /// <param name="credentialJson">
    ///     The authenticator's assertion response, as returned by <c>navigator.credentials.get</c> and serialized to JSON by the
    ///     browser-side ceremony.
    /// </param>
    /// <returns>A JSON payload indicating success, or an error message on failure.</returns>
    public async Task<IActionResult> OnPostCompletePasskeyLogin(string credentialJson)
    {
        if (!TryGetPendingPasskeyLogin(out var pending))
        {
            return new JsonResult(new { success = false, error = "Login timed out. Try again." });
        }

        Response.Cookies.Delete(PasskeyLoginCookieName);

        var assertionResponse = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(credentialJson, CamelCaseJson);
        if (assertionResponse is null)
        {
            return new JsonResult(new { success = false, error = "Malformed passkey response." });
        }

        var originalOptions = AssertionOptions.FromJson(pending.OptionsJson);
        var result = await _passkeyService.CompleteLogin(originalOptions, assertionResponse);

        if (result.IsFailed)
        {
            return new JsonResult(new { success = false, error = string.Join(' ', result.Errors.Select(e => e.Message)) });
        }

        var user = result.Value;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.GivenName, user.DisplayName),
            new(ClaimTypes.Email, user.EmailAddress)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

        return new JsonResult(new { success = true });
    }

    /// <summary>
    ///     Reads and unprotects the pending passkey login cookie, if present and not expired.
    /// </summary>
    /// <param name="pending">When this method returns, contains the pending login, if found.</param>
    /// <returns>
    ///     <see langword="true" /> if a valid, non-expired pending login was found; otherwise, <see langword="false" />.
    /// </returns>
    private bool TryGetPendingPasskeyLogin(out PendingPasskeyLogin pending)
    {
        pending = null!;

        if (!Request.Cookies.TryGetValue(PasskeyLoginCookieName, out var cookie))
        {
            return false;
        }

        try
        {
            var json = _passkeyProtector.Unprotect(cookie);
            var deserialized = JsonSerializer.Deserialize<PendingPasskeyLogin>(json);

            if (deserialized is null || DateTimeOffset.UtcNow - deserialized.IssuedAt > PasskeyChallengeLifetime)
            {
                return false;
            }

            pending = deserialized;
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Represents a passkey login ceremony's state, carried between the begin and complete requests via a short-lived,
    ///     protected cookie.
    /// </summary>
    /// <param name="OptionsJson">The assertion options issued at the start of the ceremony.</param>
    /// <param name="IssuedAt">The date and time the ceremony began.</param>
    private sealed record PendingPasskeyLogin(string OptionsJson, DateTimeOffset IssuedAt);

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
