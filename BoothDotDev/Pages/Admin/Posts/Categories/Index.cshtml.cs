using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Posts.Categories;

/// <summary>
///     Represents the page model for the admin blog post categories page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly BlogPostService _blogPostService;
    private IReadOnlyDictionary<Guid, BlogPostCategory> _byId = new Dictionary<Guid, BlogPostCategory>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    public Index(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the list of categories.
    /// </summary>
    /// <value>The list of categories, ordered by path.</value>
    public IReadOnlyList<BlogPostCategory> Categories { get; private set; } = [];

    /// <summary>
    ///     Gets the error message from a failed delete attempt, if any.
    /// </summary>
    /// <value>The error message, or <see langword="null" /> if the last delete attempt succeeded (or none was made).</value>
    [TempData]
    public string? DeleteError { get; set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        var all = _blogPostService.GetAllCategoriesFlat();
        _byId = all.ToDictionary(category => category.Id);
        Categories = [.. all.OrderBy(GetPath, StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>
    ///     Handles the POST request for deleting a category. The category must not have any child categories or posts.
    /// </summary>
    /// <param name="id">The ID of the category to delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        var result = _blogPostService.DeleteCategory(id);
        if (result.IsFailed)
        {
            DeleteError = string.Join(Environment.NewLine, result.Errors.Select(e => e.Message));
        }

        return RedirectToPage();
    }

    /// <summary>
    ///     Gets the display path of the specified category: its ancestors' names followed by its own.
    /// </summary>
    /// <param name="category">The category whose path to return.</param>
    /// <returns>The category's path, e.g. <c>Tech / Dev</c>.</returns>
    public string GetPath(BlogPostCategory category)
    {
        var names = new Stack<string>();
        for (var current = category;
             current is not null;
             current = current.ParentCategoryId is { } parentId ? _byId.GetValueOrDefault(parentId) : null)
        {
            names.Push(current.Name);
        }

        return string.Join(" / ", names);
    }
}
