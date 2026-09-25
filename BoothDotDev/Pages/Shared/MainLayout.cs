using System.Reflection;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace BoothDotDev.Pages.Shared;

/// <summary>
///     Represents the base class for all layout pages.
/// </summary>
public abstract class MainLayout : RazorPage<object>
{
    /// <summary>
    ///     Gets the current URL of the page.
    /// </summary>
    /// <value>The current URL of the page.</value>
    public Uri CurrentUrl { get; private set; } = null!;

    /// <summary>
    ///     Gets the site's own base URL, used to build absolute URLs (e.g. the Open Graph image) independent of the
    ///     current page's path.
    /// </summary>
    /// <value>The site's base URL.</value>
    public Uri SiteBaseUrl { get; private set; } = null!;

    /// <summary>
    ///     Gets or sets the Markdown rendering service.
    /// </summary>
    /// <value>The Markdown rendering service.</value>
    [RazorInject]
    public MarkdownRenderingService MarkdownRenderingService { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the service that builds the Discord component embed.
    /// </summary>
    /// <value>The Discord embed service.</value>
    [RazorInject]
    public DiscordEmbedService DiscordEmbedService { get; set; } = null!;

    /// <summary>
    ///     Gets the page title to display in the browser tab.
    /// </summary>
    /// <value>The page title.</value>
    public string PageTitle
    {
        get => ViewData["Title"] is null ? Strings.MyName : $"{ViewData["Title"]} - {Strings.MyName}";
    }

    /// <summary>
    ///     Gets the website's version string.
    /// </summary>
    /// <value>The website's version string.</value>
    public string Version { get; private set; } = "<unknown>";

    /// <summary>
    ///     Initializes the layout.
    /// </summary>
    public Task InitializeAsync()
    {
        var request = Context.Request;
        CurrentUrl = new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
        SiteBaseUrl = new Uri($"{request.Scheme}://{request.Host}");

        var attribute = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        Version = attribute?.InformationalVersion ?? "<unknown>";
        return Task.CompletedTask;
    }
}
