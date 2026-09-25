using System.Security.Claims;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

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
        get
        {
            var title = ViewData["Title"] is null ? "Admin" : $"{ViewData["Title"]} - Admin";
            return EnvironmentLabel is { } label ? $"[{label}] {title}" : title;
        }
    }

    /// <summary>
    ///     Gets the label of this deployment when it isn't the live site, such as <c>staging</c>.
    /// </summary>
    /// <value>The label, or <see langword="null" /> on the live site.</value>
    public string? EnvironmentLabel
    {
        get
        {
            var label = Context.RequestServices.GetRequiredService<IOptionsMonitor<SiteOptions>>().CurrentValue.EnvironmentLabel;
            return string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        }
    }

    /// <summary>
    ///     Gets the current user accessing the admin layout.
    /// </summary>
    /// <value>The current user.</value>
    public User CurrentUser { get; private set; } = null!;

    /// <summary>
    ///     Initializes the admin layout.
    /// </summary>
    public Task InitializeAsync()
    {
        var userService = Context.RequestServices.GetRequiredService<UserService>();
        var id = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(id, out var parsedId) ? parsedId : Guid.Empty;
        var result = userService.GetUser(userId);

        if (result.IsSuccess)
        {
            CurrentUser = result.Value;
        }

        return Task.CompletedTask;
    }
}
