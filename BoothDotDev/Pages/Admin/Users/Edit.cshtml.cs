using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;

namespace BoothDotDev.Pages.Admin.Users;

using User = Data.Models.User;

/// <summary>
///     Represents the page model for creating or editing a user in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly UserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="userService">The <see cref="UserService" />.</param>
    public Edit(UserService userService)
    {
        _userService = userService;
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
