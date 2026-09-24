using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Posts.Categories;

/// <summary>
///     Represents the page model for editing a blog post category in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly BlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    public Edit(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets or sets the category being edited, if any.
    /// </summary>
    /// <value>The category being edited, or default values if a new category is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new category is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new category is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the categories the edited category may be moved beneath.
    /// </summary>
    /// <value>Every category except the edited one and its descendants.</value>
    public IReadOnlyList<BlogPostCategory> ParentOptions { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the category to edit. If <see langword="null" />, a new category will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        var all = _blogPostService.GetAllCategoriesFlat();

        if (id is null)
        {
            CreatingNew = true;
            ParentOptions = all;
            return Page();
        }

        var category = all.FirstOrDefault(c => c.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        ParentOptions = ExcludeSelfAndDescendants(all, category.Id);
        Input = new EditModel
        {
            Name = category.Name,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId,
            Color = category.Color,
            FontStyle = category.FontStyle
        };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the category.
    /// </summary>
    /// <param name="id">The ID of the category being edited. If <see langword="null" />, a new category is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return RedisplayWithOptions(id);
        }

        var request = new BlogPostCategorySaveRequest(
            Input.Name, Input.Slug, Input.ParentCategoryId, Input.Color, Input.FontStyle);

        var result = id is null ? _blogPostService.CreateCategory(request) : _blogPostService.UpdateCategory(id.Value, request);
        return RedirectOnSuccess(result, id);
    }

    /// <summary>
    ///     Handles the POST request for deleting the category.
    /// </summary>
    /// <param name="id">The ID of the category to delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        var result = _blogPostService.DeleteCategory(id);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return RedisplayWithOptions(id);
        }

        return RedirectToPage("/Admin/Posts/Categories/Index");
    }

    private IActionResult RedirectOnSuccess(Result<BlogPostCategory> result, Guid? id)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return RedisplayWithOptions(id);
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    private PageResult RedisplayWithOptions(Guid? id)
    {
        CreatingNew = id is null;
        var all = _blogPostService.GetAllCategoriesFlat();
        ParentOptions = id is { } categoryId ? ExcludeSelfAndDescendants(all, categoryId) : all;
        return Page();
    }

    private static IReadOnlyList<BlogPostCategory> ExcludeSelfAndDescendants(IReadOnlyList<BlogPostCategory> all, Guid id)
    {
        var excluded = new HashSet<Guid> { id };
        var grew = true;

        // a category is excluded if its parent is; repeat until a pass adds nothing
        while (grew)
        {
            grew = false;
            foreach (var category in all)
            {
                if (category.ParentCategoryId is { } parentId && excluded.Contains(parentId) && excluded.Add(category.Id))
                {
                    grew = true;
                }
            }
        }

        return [.. all.Where(category => !excluded.Contains(category.Id))];
    }

    /// <summary>
    ///     Represents the model for editing a blog post category.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the display name of the category.
        /// </summary>
        /// <value>The name of the category.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the slug of the category.
        /// </summary>
        /// <value>The slug of the category.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the ID of the category's parent.
        /// </summary>
        /// <value>The parent's ID, or <see langword="null" /> if the category is top-level.</value>
        public Guid? ParentCategoryId { get; set; }

        /// <summary>
        ///     Gets or sets the colour of the category.
        /// </summary>
        /// <value>The colour, or <see langword="null" /> to derive it from the category's ID.</value>
        public PaletteHue? Color { get; set; }

        /// <summary>
        ///     Gets or sets the font style of posts in the category.
        /// </summary>
        /// <value>The font style.</value>
        public FontStyle FontStyle { get; set; } = FontStyle.SansSerif;
    }
}
