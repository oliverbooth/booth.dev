using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Portfolio;

/// <summary>
///     Represents the page model for a single item of the portfolio: a project or a creation, found by its slug.
/// </summary>
public sealed class Item : PageModel
{
    private readonly CreationService _creationService;
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Item" /> class.
    /// </summary>
    /// <param name="projectService">The <see cref="ProjectService" />.</param>
    /// <param name="creationService">The <see cref="CreationService" />.</param>
    public Item(ProjectService projectService, CreationService creationService)
    {
        _projectService = projectService;
        _creationService = creationService;
    }

    /// <summary>
    ///     Gets the project shown.
    /// </summary>
    /// <value>The project, or <see langword="null" /> if the item is a creation.</value>
    public Project? Project { get; private set; }

    /// <summary>
    ///     Gets the creation shown.
    /// </summary>
    /// <value>The creation, or <see langword="null" /> if the item is a project.</value>
    public Creation? Creation { get; private set; }

    /// <summary>
    ///     Gets the title of the item shown.
    /// </summary>
    /// <value>The project's name or the creation's title.</value>
    public string Title
    {
        get => Project?.Name ?? Creation!.Title;
    }

    /// <summary>
    ///     Gets the content the page's meta tags are built from.
    /// </summary>
    /// <value>The project or creation.</value>
    public object Content
    {
        get => (object?)Project ?? Creation!;
    }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="slug">The slug of the project or creation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(string slug)
    {
        if (_projectService.TryGetProject(slug, out var project))
        {
            Project = project;
            return Page();
        }

        var creationResult = _creationService.GetCreationBySlug(slug);
        if (creationResult.IsFailed || IsHiddenFromVisitor(creationResult.Value))
        {
            return NotFound();
        }

        Creation = creationResult.Value;
        return Page();
    }

    private bool IsHiddenFromVisitor(Creation creation)
    {
        return creation.Visibility == Visibility.Private && User.Identity?.IsAuthenticated != true;
    }
}
