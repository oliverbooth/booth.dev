namespace BoothDotDev.Data;

/// <summary>
///     Represents a request to create or update a blog post category.
/// </summary>
/// <param name="Name">The display name of the category.</param>
/// <param name="Slug">The slug of the category.</param>
/// <param name="ParentCategoryId">
///     The ID of the category's parent, or <see langword="null" /> if the category is top-level.
/// </param>
/// <param name="Color">The colour of the category, or <see langword="null" /> to derive it from the category's ID.</param>
/// <param name="FontStyle">The font style of posts in the category.</param>
public sealed record BlogPostCategorySaveRequest(
    string Name,
    string Slug,
    Guid? ParentCategoryId,
    PaletteHue? Color,
    FontStyle FontStyle);
