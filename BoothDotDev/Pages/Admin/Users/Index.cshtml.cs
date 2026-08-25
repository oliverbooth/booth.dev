using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Users;

/// <summary>
///     Represents the page model for the admin users page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly UserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="userService">The <see cref="UserService" />.</param>
    public Index(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    ///     Gets the list of users.
    /// </summary>
    /// <value>The list of users.</value>
    public IReadOnlyList<User> Users { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Users = _userService.GetAllUsers();
    }
}
