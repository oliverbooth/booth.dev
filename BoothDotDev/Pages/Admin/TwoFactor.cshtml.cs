using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using OtpNet;
using X10D.Text;

namespace BoothDotDev.Pages.Admin;

/// <summary>
///     Page model for the two-factor authentication page.
/// </summary>
public sealed class TwoFactor : PageModel
{
    private IMemoryCache _cache;
    private readonly IBlogUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TwoFactor" /> class.
    /// </summary>
    /// <param name="cache">The memory cache.</param>
    /// <param name="userService">The user service.</param>
    public TwoFactor(IMemoryCache cache, IBlogUserService userService)
    {
        _cache = cache;
        _userService = userService;
    }

    /// <summary>
    ///     Gets or sets the code entered by the user.
    /// </summary>
    /// <value>The code entered by the user.</value>
    [BindProperty]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the return URL.
    /// </summary>
    /// <value>The return URL.</value>
    [BindProperty]
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the token from the cache.
    /// </summary>
    /// <value>The token from the cache.</value>
    [BindProperty]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="returnUrl">The return URL.</param>
    public IActionResult OnGet([FromQuery] string token, [FromQuery(Name = "ReturnUrl")] string? returnUrl = null)
    {
        Token = token;
        return Page();
    }

    /// <summary>
    ///     Handles the POST request.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!_cache.TryGetValue(Token, out Guid userId))
        {
            ModelState.AddModelError(string.Empty, "The two-factor authentication session has expired. Please log in again.");
            return Page();
        }

        if (!_userService.TryGetUser(userId, out IUser? user))
        {
            _cache.Remove(Token);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please log in again.");
            return Page();
        }

        var totp = new Totp(Base32Encoding.ToBytes(user.Totp ?? string.Empty));
        bool result = totp.VerifyTotp(Code, out long _, new VerificationWindow(2, 2));
        if (!result && !string.IsNullOrWhiteSpace(user.Totp))
        {
            ModelState.AddModelError(string.Empty, "The two-factor authentication code is incorrect.");
            return Page();
        }

        _cache.Remove(Token);

        await _userService.SignInAsync(HttpContext, user);
        return Redirect(ReturnUrl.WithWhiteSpaceAlternative("/admin"));
    }
}
