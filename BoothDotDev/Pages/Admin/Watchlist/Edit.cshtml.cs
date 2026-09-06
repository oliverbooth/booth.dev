using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Watchlist;

/// <summary>
///     Represents the page model for adding or editing a watchlist item.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Edit : PageModel
{
    private readonly TmdbLookupService _tmdbLookupService;
    private readonly WatchlistService _watchlistService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="watchlistService">The watchlist service.</param>
    /// <param name="tmdbLookupService">The TMDB lookup service.</param>
    public Edit(WatchlistService watchlistService, TmdbLookupService tmdbLookupService)
    {
        _watchlistService = watchlistService;
        _tmdbLookupService = tmdbLookupService;
    }

    /// <summary>
    ///     Gets a value indicating whether a new item is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new item is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the item being edited.
    /// </summary>
    /// <value>The ID of the item being edited, or <see langword="null" /> if a new item is being created.</value>
    public Guid? Id { get; private set; }

    /// <summary>
    ///     Gets or sets the item being edited, if any.
    /// </summary>
    /// <value>The item being edited, or default values if a new item is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the item to edit. If <see langword="null" />, a new item will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            Input = new EditModel { State = WatchableState.PlanToWatch };
            return Page();
        }

        var result = _watchlistService.GetWatchableById(id.Value);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var watchable = result.Value;
        Id = watchable.Id;
        Input = new EditModel { Title = watchable.Title, Kind = watchable.Kind, State = watchable.State };
        return Page();
    }

    /// <summary>
    ///     Handles the POST request for looking up movie/TV metadata by title.
    /// </summary>
    /// <param name="query">The title to search for.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of matching candidates, or an error message if none were found.</returns>
    public async Task<IActionResult> OnPostLookupAsync(string? query, CancellationToken cancellationToken)
    {
        var result = await _tmdbLookupService.SearchAsync(query ?? string.Empty, cancellationToken);
        if (result.IsFailed)
        {
            return new JsonResult(new { error = result.Errors[0].Message });
        }

        // Kind is projected to its enum-name string (matching the <select>'s option values) rather than left as the
        // default numeric JSON, since nothing else in the config pipeline sets up string enum serialization.
        var candidates = result.Value.Select(c => new { c.Title, Kind = c.Kind.ToString(), c.Year });
        return new JsonResult(new { candidates });
    }

    /// <summary>
    ///     Handles the POST request for saving the item.
    /// </summary>
    /// <param name="id">The ID of the item being edited. If <see langword="null" />, a new item is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var title = Input.Title.Trim();
        var result = id is null
            ? _watchlistService.AddWatchable(title, Input.Kind, Input.State)
            : _watchlistService.UpdateWatchable(id.Value, title, Input.Kind, Input.State);

        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage("Index");
    }

    /// <summary>
    ///     Handles the POST request for removing the item from the watchlist.
    /// </summary>
    /// <param name="id">The ID of the item to remove.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid? id)
    {
        if (id is not { } watchableId)
        {
            return BadRequest("Save the entry before it can be deleted.");
        }

        _watchlistService.DeleteWatchable(watchableId);
        return RedirectToPage("Index");
    }

    /// <summary>
    ///     Represents the model for adding or editing a watchlist item.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the item.
        /// </summary>
        /// <value>The title of the item.</value>
        [Required]
        [StringLength(128)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the kind of the item.
        /// </summary>
        /// <value>The kind of the item.</value>
        public WatchableKind Kind { get; set; } = WatchableKind.Movie;

        /// <summary>
        ///     Gets or sets the state of the item.
        /// </summary>
        /// <value>The state of the item.</value>
        public WatchableState State { get; set; } = WatchableState.PlanToWatch;
    }
}
