using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Posts;

/// <summary>
///     Represents the page model for the admin post trash page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Trash : PageModel
{
    private readonly BlogPostService _blogPostService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    public Trash(BlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    /// <summary>
    ///     Gets the list of trashed blog posts, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed blog posts.</value>
    public IReadOnlyList<BlogPost> BlogPosts { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        BlogPosts = _blogPostService.GetTrashedPosts();
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed blog post.
    /// </summary>
    /// <param name="id">The ID of the post to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        _blogPostService.RestorePost(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting a single trashed blog post.
    /// </summary>
    /// <param name="id">The ID of the post to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDelete(Guid id)
    {
        _blogPostService.PermanentlyDeletePost(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting every selected trashed blog post.
    /// </summary>
    /// <param name="ids">The IDs of the posts to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDeleteBulk(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            _blogPostService.PermanentlyDeletePost(id);
        }

        return RedirectToPage();
    }
}
