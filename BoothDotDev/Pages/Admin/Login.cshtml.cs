using System.Security.Claims;
using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using X10D.Text;

namespace BoothDotDev.Pages.Admin;

internal sealed class Login : PageModel
{
    private readonly IBlogUserService _blogUserService;
    private readonly IMemoryCache _cache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Login" /> class.
    /// </summary>
    /// <param name="blogUserService">The blog user service.</param>
    /// <param name="cache">The memory cache.</param>
    public Login(IBlogUserService blogUserService, IMemoryCache cache)
    {
        _blogUserService = blogUserService;
        _cache = cache;
    }

    /// <summary>
    ///     Gets or sets the email address of the user attempting to log in.
    /// </summary>
    /// <value>The email address of the user attempting to log in.</value>
    [BindProperty]
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the password of the user attempting to log in.
    /// </summary>
    /// <value>The password of the user attempting to log in.</value>
    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync([FromQuery(Name = "ReturnUrl")] string? returnUrl = null)
    {
        if (!_blogUserService.TryGetUser(EmailAddress, out IUser? user) || !user.TestCredentials(Password))
        {
            ModelState.AddModelError(string.Empty, "The email address or password is incorrect.");
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(user.Totp))
        {
            var token = Guid.NewGuid().ToString("N");
            _cache.Set(token, user.Id, TimeSpan.FromMinutes(2));
            return RedirectToPage("/Admin/TwoFactor", new { token, ReturnUrl = returnUrl });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.EmailAddress)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return Redirect(returnUrl.WithWhiteSpaceAlternative("/admin"));
    }
}
