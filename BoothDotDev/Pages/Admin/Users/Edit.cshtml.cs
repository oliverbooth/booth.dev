using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using BoothDotDev.Data;
using BoothDotDev.Services;
using Fido2NetLib;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;

namespace BoothDotDev.Pages.Admin.Users;

using PasskeyCredential = Data.Models.PasskeyCredential;
using User = Data.Models.User;

/// <summary>
///     Represents the page model for creating or editing a user in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private const string PasskeyRegistrationCookieName = "webauthn_reg_challenge";
    private static readonly TimeSpan PasskeyChallengeLifetime = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly UserService _userService;
    private readonly PasskeyService _passkeyService;
    private readonly IDataProtector _protector;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="userService">The <see cref="UserService" />.</param>
    /// <param name="passkeyService">The <see cref="PasskeyService" />.</param>
    /// <param name="provider">The <see cref="IDataProtectionProvider" /> used to create a data protector.</param>
    public Edit(UserService userService, PasskeyService passkeyService, IDataProtectionProvider provider)
    {
        _userService = userService;
        _passkeyService = passkeyService;
        _protector = provider.CreateProtector("AdminUsers.PasskeyRegistration");
    }

    /// <summary>
    ///     Gets or sets the user being edited, if any.
    /// </summary>
    /// <value>The user being edited, or default values if a new user is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets the avatar of the user being edited.
    /// </summary>
    /// <value>The avatar of the user being edited, or <see langword="null" /> if a new user is being created.</value>
    public Uri? AvatarUrl { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether a new user is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new user is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the user being edited has TOTP configured.
    /// </summary>
    /// <value><see langword="true" /> if the user has TOTP configured; otherwise, <see langword="false" />.</value>
    public bool HasTotp { get; private set; }

    /// <summary>
    ///     Gets the SVG markup for a QR code encoding <see cref="Input" />'s current TOTP secret, suitable for
    ///     scanning into an authenticator app.
    /// </summary>
    /// <value>The QR code SVG markup, or <see langword="null" /> if there's no secret to encode.</value>
    public string? TotpQrCodeSvg { get; private set; }

    /// <summary>
    ///     Gets the user's registered passkeys, newest first.
    /// </summary>
    /// <value>The user's registered passkeys.</value>
    public IReadOnlyList<PasskeyCredential> Passkeys { get; private set; } = [];

    /// <summary>
    ///     Gets the ID of the user being edited.
    /// </summary>
    /// <value>The ID of the user being edited, or <see langword="null" /> if a new user is being created.</value>
    public Guid? UserId { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the user to edit. If <see langword="null" />, a new user will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            return Page();
        }

        var result = _userService.GetUser(id.Value);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var user = result.Value;
        LoadDisplayState(user);
        Input = new EditModel
        {
            DisplayName = user.DisplayName,
            EmailAddress = user.EmailAddress,
            DisableLogin = string.IsNullOrWhiteSpace(user.Password),
            TotpSecret = user.TotpSecret
        };

        UpdateTotpQrCode();
        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the user.
    /// </summary>
    /// <param name="id">The ID of the user being edited. If <see langword="null" />, a new user is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            ReloadDisplayState(id);
            UpdateTotpQrCode();
            return Page();
        }

        var request = new UserSaveRequest(Input.DisplayName, Input.EmailAddress, Input.DisableLogin, Input.NewPassword,
            Input.TotpSecret);
        var result = id is null ? _userService.CreateUser(request) : _userService.UpdateUser(id.Value, request);

        return RedirectOnSuccess(id, result);
    }

    /// <summary>
    ///     Handles the POST request for resetting the user's TOTP, forcing re-enrollment.
    /// </summary>
    /// <param name="id">The ID of the user whose TOTP to reset.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostResetTotp(Guid? id)
    {
        if (id is not { } userId)
        {
            return BadRequest("Save the user before resetting TOTP.");
        }

        return RedirectOnSuccess(userId, _userService.ResetTotp(userId));
    }

    /// <summary>
    ///     Handles the POST request for generating a new random TOTP secret into the form, without saving it. The
    ///     admin reviews the QR code and hits Save changes separately to actually commit it.
    /// </summary>
    /// <param name="id">The ID of the user being edited. If <see langword="null" />, a new user is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostGenerateTotp(Guid? id)
    {
        CreatingNew = id is null;
        ReloadDisplayState(id);

        Input.TotpSecret = UserService.GenerateTotpSecret();
        UpdateTotpQrCode();
        return Page();
    }

    /// <summary>
    ///     Handles the POST request for beginning a passkey registration ceremony.
    /// </summary>
    /// <param name="id">The ID of the user to register a new passkey for.</param>
    /// <param name="nickname">A user-supplied label for the new passkey.</param>
    /// <returns>
    ///     The credential creation options for the browser to pass to <c>navigator.credentials.create</c>, as raw JSON.
    /// </returns>
    public IActionResult OnPostBeginPasskeyRegistration(Guid? id, string? nickname)
    {
        if (id is not { } userId)
        {
            return BadRequest("Save the user before registering a passkey.");
        }

        var userResult = _userService.GetUser(userId);
        if (userResult.IsFailed)
        {
            return NotFound();
        }

        var options = _passkeyService.BeginRegistration(userResult.Value);
        var pending = new PendingPasskeyRegistration(nickname, options.ToJson(), DateTimeOffset.UtcNow);
        var payload = _protector.Protect(JsonSerializer.Serialize(pending));

        Response.Cookies.Append(PasskeyRegistrationCookieName, payload,
            new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, MaxAge = PasskeyChallengeLifetime
            });

        return Content(options.ToJson(), "application/json");
    }

    /// <summary>
    ///     Handles the POST request for completing a passkey registration ceremony.
    /// </summary>
    /// <param name="id">The ID of the user the passkey is being registered for.</param>
    /// <param name="credentialJson">
    ///     The authenticator's attestation response, as returned by <c>navigator.credentials.create</c> and serialized to JSON by
    ///     the browser-side ceremony.
    /// </param>
    /// <returns>A JSON payload indicating success, or an error message on failure.</returns>
    public async Task<IActionResult> OnPostCompletePasskeyRegistration(Guid? id, string credentialJson)
    {
        if (id is not { } userId)
        {
            return BadRequest("Save the user before registering a passkey.");
        }

        var userResult = _userService.GetUser(userId);
        if (userResult.IsFailed)
        {
            return NotFound();
        }

        if (!TryGetPendingRegistration(out var pending))
        {
            return new JsonResult(new { success = false, error = "Registration timed out. Try again." });
        }

        Response.Cookies.Delete(PasskeyRegistrationCookieName);

        var attestationResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(credentialJson, CamelCaseJson);
        if (attestationResponse is null)
        {
            return new JsonResult(new { success = false, error = "Malformed passkey response." });
        }

        var originalOptions = CredentialCreateOptions.FromJson(pending.OptionsJson);
        var result = await _passkeyService.CompleteRegistration(userResult.Value, originalOptions, attestationResponse, pending.Nickname);

        return new JsonResult(result.IsSuccess
            ? new { success = true }
            : new { success = false, error = string.Join(' ', result.Errors.Select(e => e.Message)) });
    }

    /// <summary>
    ///     Handles the POST request for deleting a passkey.
    /// </summary>
    /// <param name="id">The ID of the user the passkey belongs to.</param>
    /// <param name="credentialId">The ID of the passkey to delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDeletePasskey(Guid? id, Guid credentialId)
    {
        if (id is not { } userId)
        {
            return BadRequest("Save the user before managing passkeys.");
        }

        _passkeyService.DeleteCredential(credentialId);
        return RedirectToPage(new { id = userId });
    }

    /// <summary>
    ///     Reads and unprotects the pending passkey registration cookie, if present and not expired.
    /// </summary>
    /// <param name="pending">When this method returns, contains the pending registration, if found.</param>
    /// <returns>
    ///     <see langword="true" /> if a valid, non-expired pending registration was found; otherwise, <see langword="false" />.
    /// </returns>
    private bool TryGetPendingRegistration(out PendingPasskeyRegistration pending)
    {
        pending = default!;

        if (!Request.Cookies.TryGetValue(PasskeyRegistrationCookieName, out var cookie))
        {
            return false;
        }

        try
        {
            var json = _protector.Unprotect(cookie);
            var deserialized = JsonSerializer.Deserialize<PendingPasskeyRegistration>(json);

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
    ///     Represents a passkey registration ceremony's state, carried between the begin and complete requests via a short-lived,
    ///     protected cookie.
    /// </summary>
    /// <param name="Nickname">The user-supplied label for the passkey being registered.</param>
    /// <param name="OptionsJson">The credential creation options issued at the start of the ceremony.</param>
    /// <param name="IssuedAt">The date and time the ceremony began.</param>
    private sealed record PendingPasskeyRegistration(string? Nickname, string OptionsJson, DateTimeOffset IssuedAt);

    /// <summary>
    ///     Reloads <see cref="AvatarUrl" />, <see cref="HasTotp" />, and <see cref="UserId" /> for an existing user,
    ///     for handlers that need to re-render the page without having gone through <see cref="OnGet" /> first.
    /// </summary>
    /// <param name="id">The ID of the user being edited. If <see langword="null" />, this is a no-op.</param>
    private void ReloadDisplayState(Guid? id)
    {
        if (id is not { } userId)
        {
            return;
        }

        var result = _userService.GetUser(userId);
        if (result.IsSuccess)
        {
            LoadDisplayState(result.Value);
        }
    }

    /// <summary>
    ///     Sets <see cref="AvatarUrl" />, <see cref="HasTotp" />, and <see cref="UserId" /> from a loaded user.
    /// </summary>
    private void LoadDisplayState(User user)
    {
        UserId = user.Id;
        AvatarUrl = user.GetAvatarUrl(36);
        HasTotp = !string.IsNullOrWhiteSpace(user.TotpSecret);
        Passkeys = _passkeyService.ListCredentials(user.Id);
    }

    /// <summary>
    ///     Recomputes <see cref="TotpQrCodeSvg" /> from <see cref="Input" />'s current TOTP secret and email address.
    /// </summary>
    private void UpdateTotpQrCode()
    {
        if (string.IsNullOrWhiteSpace(Input.TotpSecret) || string.IsNullOrWhiteSpace(Input.EmailAddress))
        {
            TotpQrCodeSvg = null;
            return;
        }

        var otp = new PayloadGenerator.OneTimePassword
        {
            Secret = Input.TotpSecret,
            Issuer = Strings.MyName,
            Label = Input.EmailAddress,
            Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.TOTP
        };

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(otp.ToString(), QRCodeGenerator.ECCLevel.Q);
        var svgQrCode = new SvgQRCode(data);

        // Fixed black-on-white regardless of site theme - QR scanners rely on high contrast, and a dark-mode
        // inversion here would just make it unreliable to scan for no real benefit.
        TotpQrCodeSvg = svgQrCode.GetGraphic(5, "#000000", "#ffffff");
    }

    /// <summary>
    ///     Redirects back to this user's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="id">The ID of the user being edited, to reload display state for on failure. If <see langword="null" />, a new user was being created.</param>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Guid? id, Result<User> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            ReloadDisplayState(id);
            UpdateTotpQrCode();
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for creating or editing a user.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the display name of the user.
        /// </summary>
        /// <value>The display name of the user.</value>
        [Required]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the email address of the user.
        /// </summary>
        /// <value>The email address of the user.</value>
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a value indicating whether login should be disabled for this user, clearing any
        ///     existing password.
        /// </summary>
        /// <value><see langword="true" /> if login should be disabled; otherwise, <see langword="false" />.</value>
        public bool DisableLogin { get; set; }

        /// <summary>
        ///     Gets or sets the new password for the user, or <see langword="null" />/whitespace to leave the
        ///     existing password (if any) unchanged. Ignored when <see cref="DisableLogin" /> is <see langword="true" />.
        /// </summary>
        /// <value>The new password for the user.</value>
        public string? NewPassword { get; set; }

        /// <summary>
        ///     Gets or sets the user's TOTP secret, or <see langword="null" /> if TOTP isn't configured.
        /// </summary>
        /// <value>The user's TOTP secret.</value>
        public string? TotpSecret { get; set; }
    }
}
