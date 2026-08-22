using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Tutorials.Folders;

/// <summary>
///     Represents the page model for editing a tutorial folder in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="tutorialService">The tutorial service.</param>
    public Edit(TutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    /// <summary>
    ///     Gets or sets the folder being edited, if any.
    /// </summary>
    /// <value>The folder being edited, or default values if a new folder is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new folder is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new folder is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the folder being edited.
    /// </summary>
    /// <value>The ID of the folder being edited, or <see langword="null" /> if a new folder is being created.</value>
    public Guid? FolderId { get; private set; }

    /// <summary>
    ///     Gets the folders eligible to be this folder's parent, i.e. every folder except itself and its own
    ///     descendants (to prevent creating a cycle).
    /// </summary>
    /// <value>The eligible parent folders.</value>
    public IReadOnlyList<TutorialFolder> ParentOptions { get; private set; } = [];

    /// <summary>
    ///     Gets the full slug path of the specified folder, for display in the parent picker.
    /// </summary>
    /// <param name="folder">The folder whose path to return.</param>
    /// <returns>The folder's full slug path.</returns>
    public string GetPath(TutorialFolder folder)
    {
        return _tutorialService.GetFullSlug(folder);
    }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the folder to edit. If <see langword="null" />, a new folder will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        var allFolders = _tutorialService.GetAllFolders();

        if (id is null)
        {
            CreatingNew = true;
            ParentOptions = allFolders;
            Input = new EditModel { Visibility = Visibility.Published };
            return Page();
        }

        var folderResult = _tutorialService.GetFolder(id.Value);
        if (folderResult.IsFailed)
        {
            return NotFound();
        }

        var folder = folderResult.Value;
        FolderId = folder.Id;
        ParentOptions = [.. allFolders.Where(f => f.Id != folder.Id && !IsDescendant(f, folder.Id, allFolders))];
        Input = new EditModel
        {
            Title = folder.Title,
            Slug = folder.Slug,
            Description = folder.Description,
            PreviewImageUrl = folder.PreviewImageUrl?.ToString(),
            Visibility = folder.Visibility,
            Rank = folder.Rank,
            Parent = folder.Parent
        };

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the folder.
    /// </summary>
    /// <param name="id">The ID of the folder being edited. If <see langword="null" />, a new folder is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;
        FolderId = id;

        if (!ModelState.IsValid)
        {
            ParentOptions = _tutorialService.GetAllFolders();
            return Page();
        }

        Uri? previewImageUrl = Uri.TryCreate(Input.PreviewImageUrl, UriKind.Absolute, out var uri) ? uri : null;
        var request = new TutorialFolderSaveRequest(
            Input.Title,
            Input.Slug,
            Input.Description,
            previewImageUrl,
            Input.Visibility,
            Input.Rank,
            Input.Parent);

        var result = id is null ? _tutorialService.CreateFolder(request) : _tutorialService.UpdateFolder(id.Value, request);
        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for deleting the folder. The folder must not contain any child folders or
    ///     articles.
    /// </summary>
    /// <param name="id">The ID of the folder to delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        var result = _tutorialService.DeleteFolder(id);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            FolderId = id;
            var folderResult = _tutorialService.GetFolder(id);
            var allFolders = _tutorialService.GetAllFolders();

            if (folderResult.IsSuccess)
            {
                var folder = folderResult.Value;
                ParentOptions = [.. allFolders.Where(f => f.Id != folder.Id && !IsDescendant(f, folder.Id, allFolders))];
                Input = new EditModel
                {
                    Title = folder.Title,
                    Slug = folder.Slug,
                    Description = folder.Description,
                    PreviewImageUrl = folder.PreviewImageUrl?.ToString(),
                    Visibility = folder.Visibility,
                    Rank = folder.Rank,
                    Parent = folder.Parent
                };
            }

            return Page();
        }

        return RedirectToPage("/Admin/Tutorials/Folders/Index");
    }

    /// <summary>
    ///     Redirects back to this folder's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<TutorialFolder> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            ParentOptions = _tutorialService.GetAllFolders();
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Determines whether a folder is a descendant of another, by walking up its <see cref="TutorialFolder.Parent" />
    ///     chain.
    /// </summary>
    /// <param name="candidate">The folder to test.</param>
    /// <param name="ancestorId">The ID of the potential ancestor.</param>
    /// <param name="allFolders">Every folder, used to resolve parent lookups without extra round trips.</param>
    /// <returns><see langword="true" /> if <paramref name="candidate" /> descends from <paramref name="ancestorId" />.</returns>
    private static bool IsDescendant(TutorialFolder candidate, Guid ancestorId, IReadOnlyList<TutorialFolder> allFolders)
    {
        var current = candidate;

        while (current.Parent is { } parentId)
        {
            if (parentId == ancestorId)
            {
                return true;
            }

            current = allFolders.FirstOrDefault(f => f.Id == parentId);
            if (current is null)
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    ///     Represents the model for editing a tutorial folder.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the folder.
        /// </summary>
        /// <value>The title of the folder.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the slug of the folder.
        /// </summary>
        /// <value>The slug of the folder.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the description of the folder.
        /// </summary>
        /// <value>The description, or <see langword="null" /> if the folder has no description.</value>
        public string? Description { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the folder's preview image.
        /// </summary>
        /// <value>The preview image URL, or <see langword="null" /> if the folder has no preview image.</value>
        public string? PreviewImageUrl { get; set; }

        /// <summary>
        ///     Gets or sets the visibility of the folder.
        /// </summary>
        /// <value>The visibility of the folder.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;

        /// <summary>
        ///     Gets or sets the rank of the folder within its parent.
        /// </summary>
        /// <value>The rank.</value>
        public int Rank { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the folder's parent.
        /// </summary>
        /// <value>The ID of the parent, or <see langword="null" /> if the folder is at the root.</value>
        public Guid? Parent { get; set; }
    }
}
