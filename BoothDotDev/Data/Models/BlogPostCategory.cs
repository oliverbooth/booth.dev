using System.ComponentModel.DataAnnotations.Schema;
using BoothDotDev.Extensions;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a category for blog posts.
/// </summary>
public sealed class BlogPostCategory
{
    /// <summary>
    ///     Gets or sets the colour for the blog post category, or <see langword="null" /> to fall back to a hash of
    ///     <see cref="Id" />.
    /// </summary>
    /// <value>The explicitly-assigned colour, or <see langword="null" />.</value>
    public PaletteHue? Color { get; set; }

    /// <summary>
    ///     Gets the colour this category renders with: <see cref="Color" /> if set, otherwise a hash of <see cref="Id" />.
    /// </summary>
    /// <value>The effective colour.</value>
    [NotMapped]
    public PaletteHue EffectiveColor
    {
        get => Color ?? PaletteHueExtensions.HashFrom(Id);
    }

    /// <summary>
    ///     Gets or sets the font style for the blog post category.
    /// </summary>
    /// <value>The font style for the blog post category.</value>
    public FontStyle FontStyle { get; set; } = FontStyle.SansSerif;

    /// <summary>
    ///     Gets or sets the unique identifier for the blog post category.
    /// </summary>
    /// <value>The unique identifier for the blog post category.</value>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the name of the blog post category.
    /// </summary>
    /// <value>The name of the blog post category.</value>
    public string Name { get; set; } = "";

    /// <summary>
    ///     Gets or sets the slug for the blog post category.
    /// </summary>
    /// <value>The slug for the blog post category.</value>
    public string Slug { get; set; } = "";

    /// <summary>
    ///     Gets or sets the unique identifier of the parent category, if any.
    /// </summary>
    /// <value>The unique identifier of the parent category, if any.</value>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    ///     Gets or sets the parent category, if any.
    /// </summary>
    /// <value>The parent category, if any.</value>
    public BlogPostCategory? ParentCategory { get; set; }

    /// <summary>
    ///     Gets or sets the child categories.
    /// </summary>
    /// <value>The child categories.</value>
    public ICollection<BlogPostCategory> Children { get; set; } = [];
}
