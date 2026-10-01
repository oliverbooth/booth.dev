using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Comments;

/// <summary>
///     Represents the page model for editing a legacy comment author.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Author : PageModel
{
    private readonly CommentService _commentService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Author" /> class.
    /// </summary>
    /// <param name="commentService">The comment service.</param>
    public Author(CommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    ///     Gets or sets the current name of the author being edited.
    /// </summary>
    /// <value>The author's name.</value>
    [BindProperty(SupportsGet = true)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name the author is being renamed to.
    /// </summary>
    /// <value>The new name.</value>
    [BindProperty]
    public string NewName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the avatar the author's comments carry.
    /// </summary>
    /// <value>The avatar, or <see langword="null" /> if the author has none.</value>
    public string? Avatar { get; private set; }

    /// <summary>
    ///     Gets the author's comments.
    /// </summary>
    /// <value>The comments, oldest first.</value>
    public IReadOnlyList<LegacyComment> Comments { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet()
    {
        NewName = Name;
        return Load() ? Page() : NotFound();
    }

    /// <summary>
    ///     Handles the POST request for renaming the author on all of their comments.
    /// </summary>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRename()
    {
        var newName = NewName?.Trim() ?? string.Empty;
        if (newName.Length is 0 or > 50)
        {
            ModelState.AddModelError(nameof(NewName), "The name must be between 1 and 50 characters.");
            return Load() ? Page() : NotFound();
        }

        _commentService.RenameLegacyAuthor(Name, newName);
        return RedirectToPage(new {name = newName});
    }

    /// <summary>
    ///     Handles the POST request for replacing the author's avatar.
    /// </summary>
    /// <param name="file">The uploaded image.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile? file)
    {
        if (file is null)
        {
            ModelState.AddModelError(string.Empty, "Choose a file to upload.");
            return Load() ? Page() : NotFound();
        }

        await using var stream = file.OpenReadStream();
        if (await CommentService.EncodeAvatarAsync(stream) is not { } avatar)
        {
            ModelState.AddModelError(string.Empty, "That file isn't an image that can be encoded as an avatar.");
            return Load() ? Page() : NotFound();
        }

        _commentService.SetLegacyAuthorAvatar(Name, avatar);
        return RedirectToPage(new {name = Name});
    }

    /// <summary>
    ///     Handles the POST request for removing the author's avatar.
    /// </summary>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostClearAvatar()
    {
        _commentService.SetLegacyAuthorAvatar(Name, null);
        return RedirectToPage(new {name = Name});
    }

    private bool Load()
    {
        Comments = _commentService.GetLegacyCommentsByAuthor(Name);
        Avatar = Comments.Select(c => c.Avatar).FirstOrDefault(a => !string.IsNullOrEmpty(a));
        return Comments.Count > 0;
    }
}
