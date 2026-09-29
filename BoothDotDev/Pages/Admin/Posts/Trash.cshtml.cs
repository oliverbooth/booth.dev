using BoothDotDev.Data.Models;
using BoothDotDev.Services;

namespace BoothDotDev.Pages.Admin.Posts;

/// <summary>
///     Represents the page model for the admin post trash page.
/// </summary>
public sealed class Trash : TrashPageModel<BlogPost>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    public Trash(BlogPostService blogPostService)
        : base(blogPostService.GetTrashedPosts, id => blogPostService.RestorePost(id), id => blogPostService.PermanentlyDeletePost(id))
    {
    }

    /// <summary>
    ///     Gets the list of trashed blog posts, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed blog posts.</value>
    public IReadOnlyList<BlogPost> BlogPosts => Items;
}
