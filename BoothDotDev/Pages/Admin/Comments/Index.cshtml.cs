using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Comments;

/// <summary>
///     Represents the page model for the admin legacy comments page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly CommentService _commentService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="commentService">The comment service.</param>
    public Index(CommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    ///     Gets the legacy comment authors.
    /// </summary>
    /// <value>The authors.</value>
    public IReadOnlyList<CommentService.LegacyAuthor> Authors { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Authors = _commentService.GetLegacyAuthors();
    }
}
