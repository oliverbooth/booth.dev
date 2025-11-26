using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using X10D.Text;

namespace BoothDotDev.Pages.Admin;

internal sealed class Login : PageModel
{
    private readonly IBlogUserService _userService;
    private readonly IMemoryCache _cache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Login" /> class.
    /// </summary>
    /// <param name="cache">The memory cache.</param>
    /// <param name="userService">The blog user service.</param>
    public Login(IMemoryCache cache, IBlogUserService userService)
    {
        _userService = userService;
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
        if (!_userService.TryGetUser(EmailAddress, out IUser? user) || !user.TestCredentials(Password))
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

        await _userService.SignInAsync(HttpContext, user);
        return Redirect(returnUrl.WithWhiteSpaceAlternative("/admin"));
    }
}
