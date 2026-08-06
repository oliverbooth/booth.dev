using System.Reflection;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Razor;

namespace BoothDotDev.Pages.Shared;

/// <summary>
///     Represents the base class for all layout pages.
/// </summary>
public abstract class MainLayout : RazorPage<object>
{
    /// <summary>
    ///     Gets or sets the blog post service.
    /// </summary>
    /// <value>The blog post service.</value>
    [Inject]
    public IBlogPostService BlogPostService { get; set; } = null!;

    /// <summary>
    ///     Gets the current URL of the page.
    /// </summary>
    /// <value>The current URL of the page.</value>
    public Uri CurrentUrl { get; private set; } = null!;

    /// <summary>
    ///     Gets the page title to display in the browser tab.
    /// </summary>
    /// <value>The page title.</value>
    public string PageTitle
    {
        get => ViewData["Title"] is null ? Strings.MyName : $"{ViewData["Title"]} - {Strings.MyName}";
    }

    /// <summary>
    ///     Gets the source code of the current page for display in the Quine section.
    /// </summary>
    /// <value>The source code of the current page.</value>
    public string? QuineSource { get; private set; }

    /// <summary>
    ///    Gets or sets the tutorial service.
    /// </summary>
    /// <value>The tutorial service.</value>
    [Inject]
    public ITutorialService TutorialService { get; set; } = null!;

    /// <summary>
    ///     Gets the website's version string.
    /// </summary>
    /// <value>The website's version string.</value>
    public string Version { get; private set; } = "<unknown>";

    /// <summary>
    ///     Initializes the layout.
    /// </summary>
    public async Task InitializeAsync()
    {
        var request = Context.Request;
        CurrentUrl = new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}");

        var env = Context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var descriptor = ViewContext.ActionDescriptor as Microsoft.AspNetCore.Mvc.RazorPages.CompiledPageActionDescriptor;

        if (descriptor?.RelativePath is { } relativePath)
        {
            var file = env.ContentRootFileProvider.GetFileInfo(relativePath);
            if (file.Exists)
            {
                using var reader = new StreamReader(file.CreateReadStream());
                QuineSource = await reader.ReadToEndAsync();
            }
        }

        var attribute = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        Version = attribute?.InformationalVersion ?? "<unknown>";
    }
}
