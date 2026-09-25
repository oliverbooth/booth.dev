using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Pages.Admin.Portfolio;
using BoothDotDev.Pages.Shared.Partials;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Creations;

/// <summary>
///     Represents the page model for editing a creation in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly CreationService _creationService;
    private readonly MediaService _mediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="creationService">The <see cref="CreationService" />.</param>
    /// <param name="mediaService">The <see cref="MediaService" />.</param>
    public Edit(CreationService creationService, MediaService mediaService)
    {
        _creationService = creationService;
        _mediaService = mediaService;
    }

    /// <summary>
    ///     Gets or sets the creation being edited, if any.
    /// </summary>
    /// <value>The creation being edited, or default values if a new creation is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new creation is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new creation is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the creation being edited.
    /// </summary>
    /// <value>The ID, or <see langword="null" /> if a new creation is being created.</value>
    public Guid? ItemId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the creation being edited is in the trash.
    /// </summary>
    /// <value><see langword="true" /> if the creation is trashed; otherwise, <see langword="false" />.</value>
    public bool IsTrashed { get; private set; }

    /// <summary>
    ///     Gets what the media manager shows: the creation's files.
    /// </summary>
    /// <value>The media manager, or <see langword="null" /> if a new creation is being created.</value>
    public MediaManager? Media { get; private set; }

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the creation to edit. If <see langword="null" />, a new creation will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        if (id is null)
        {
            CreatingNew = true;
            Input = new EditModel { Visibility = Visibility.Published, PublishedAt = DateTimeOffset.UtcNow.ToLocalTime() };
            return Page();
        }

        var itemResult = _creationService.GetCreation(id.Value, true);
        if (itemResult.IsFailed)
        {
            return NotFound();
        }

        Populate(itemResult.Value);
        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the creation's fields.
    /// </summary>
    /// <param name="id">The ID of the creation being edited. If <see langword="null" />, a new creation is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    /// <remarks>The creation's files are untouched. They're managed separately by the media manager.</remarks>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;
        ItemId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = new CreationSaveRequest(
            Input.Kind,
            Input.Title,
            Input.Description,
            Input.PublishedAt,
            Input.Visibility,
            Input.IsWorkInProgress,
            Input.MadeWith);

        var result = id is null
            ? _creationService.CreateCreation(request)
            : _creationService.UpdateCreation(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for moving the creation to the trash.
    /// </summary>
    /// <param name="id">The ID of the creation to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        ItemId = id;
        return RedirectOnSuccess(_creationService.TrashCreation(id));
    }

    /// <summary>
    ///     Handles the POST request for restoring the creation from the trash.
    /// </summary>
    /// <param name="id">The ID of the creation to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(Guid id)
    {
        ItemId = id;
        return RedirectOnSuccess(_creationService.RestoreCreation(id));
    }

    /// <summary>
    ///     Populates the page from the given creation.
    /// </summary>
    /// <param name="item">The creation to populate from.</param>
    private void Populate(Creation item)
    {
        ItemId = item.Id;
        IsTrashed = item.TrashedAt is not null;
        Input = new EditModel
        {
            Kind = item.Kind,
            Title = item.Title,
            Description = item.Description,
            PublishedAt = item.PublishedAt.ToLocalTime(),
            Visibility = item.Visibility,
            IsWorkInProgress = item.IsWorkInProgress,
            MadeWith = item.MadeWith
        };

        Media = new MediaManager
        {
            OwnerId = item.Id,
            Items = _mediaService.GetMedia(MediaOwner.For(item)),
            ReturnUrl = Url.Page("/Admin/Creations/Edit", new { id = item.Id })!,
            Errors = TempData[MediaHandler.ErrorsKey] as string
        };
    }

    /// <summary>
    ///     Redirects back to this creation's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<Creation> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a creation.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the kind of the creation.
        /// </summary>
        /// <value>The kind.</value>
        public CreationKind Kind { get; set; } = CreationKind.Drawing;

        /// <summary>
        ///     Gets or sets the title of the creation.
        /// </summary>
        /// <value>The title.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the description of the creation.
        /// </summary>
        /// <value>The description, or <see langword="null" /> if the creation has no description.</value>
        public string? Description { get; set; }

        /// <summary>
        ///     Gets or sets the publication date and time of the creation.
        /// </summary>
        /// <value>The publication date and time.</value>
        public DateTimeOffset PublishedAt { get; set; }

        /// <summary>
        ///     Gets or sets the visibility of the creation.
        /// </summary>
        /// <value>The visibility.</value>
        public Visibility Visibility { get; set; } = Visibility.Published;

        /// <summary>
        ///     Gets or sets a value indicating whether the creation is a work in progress.
        /// </summary>
        /// <value><see langword="true" /> if the creation is a work in progress; otherwise, <see langword="false" />.</value>
        public bool IsWorkInProgress { get; set; }

        /// <summary>
        ///     Gets or sets a string describing how the creation was made.
        /// </summary>
        /// <value>The "made with" string, or <see langword="null" /> if not specified.</value>
        public string? MadeWith { get; set; }
    }
}
