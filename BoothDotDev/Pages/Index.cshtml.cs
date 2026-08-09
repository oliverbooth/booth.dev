using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages;

internal sealed class Index : PageModel
{
    private readonly BlogPostService _blogPostService;
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index"/> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="projectService">The project service.</param>
    public Index(BlogPostService blogPostService, ProjectService projectService)
    {
        _blogPostService = blogPostService;
        _projectService = projectService;
    }

    /// <summary>
    ///     Gets the latest blog posts.
    /// </summary>
    /// <returns>The latest blog posts.</returns>
    public IReadOnlyList<BlogPost> BlogPosts { get; private set; } = [];

    /// <summary>
    ///     Gets the latest projects.
    /// </summary>
    /// <returns>The latest projects.</returns>
    public IReadOnlyList<Project> Projects { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request for the index page.
    /// </summary>
    public void OnGet()
    {
        BlogPosts = _blogPostService.GetBlogPosts(0, 3);
        Projects = [.. _projectService.GetProjects().Concat(_projectService.GetProjects(ProjectStatus.Past)).Take(3)];
    }
}
