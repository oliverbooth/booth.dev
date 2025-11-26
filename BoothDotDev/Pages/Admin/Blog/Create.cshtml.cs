using System.Security.Claims;
using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Blog;

/// <summary>
///     Represents a class which defines the model for the <c>/admin/blog/create</c> route.
/// </summary>
[Authorize]
public sealed class Create : PageModel
{
    private readonly IBlogPostService _blogPostService;
    private readonly IBlogUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Create" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="userService">The blog user service.</param>
    public Create(IBlogPostService blogPostService, IBlogUserService userService)
    {
        _blogPostService = blogPostService;
        _userService = userService;
    }

    /// <summary>
    ///     Handles the incoming GET request to the page.
    /// </summary>
    /// <returns>A redirection to the blog post edit page for the newly-created post.</returns>
    public IActionResult OnGet()
    {
        // get user identity from claims
        Claim? userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null ||
            !Guid.TryParse(userIdClaim.Value, out Guid userId) ||
            !_userService.TryGetUser(userId, out IUser? user))
        {
            return Forbid();
        }

        var editModel = new BlogPostEditModel { AuthorId = user.Id };
        IBlogPost post = _blogPostService.CreateBlogPost(editModel);
        return RedirectToPage("/Admin/Blog/Edit", new { id = post.Id });
    }
}
