using System.Reflection;
using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.Options;

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
        get
        {
            var title = ViewData["Title"] is null ? Strings.MyName : $"{ViewData["Title"]} - {Strings.MyName}";
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
