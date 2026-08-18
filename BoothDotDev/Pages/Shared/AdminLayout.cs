using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Razor;

namespace BoothDotDev.Pages.Shared;

/// <summary>
///     Represents the base class for all admin layout pages.
/// </summary>
public abstract class AdminLayout : RazorPage<object>
{
    /// <summary>
    ///     Gets the page title to display in the browser tab.
    /// </summary>
    /// <value>The page title.</value>
    public string PageTitle
    {
        get => ViewData["Title"] is null ? "Admin" : $"{ViewData["Title"]} - Admin";
    }

    /// <summary>
    ///     Gets the display name of the currently signed-in admin user.
    /// </summary>
    /// <value>The display name of the currently signed-in admin user.</value>
    public string CurrentUserDisplayName { get; private set; } = string.Empty;

    /// <summary>
    ///     Initializes the admin layout.
    /// </summary>
    public Task InitializeAsync()
    {
        CurrentUserDisplayName = Context.User.FindFirstValue(ClaimTypes.GivenName) ?? "Admin";
        return Task.CompletedTask;
    }
}
