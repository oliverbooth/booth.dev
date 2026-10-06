using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BoothDotDev.Data;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Gamelist;

/// <summary>
///     Represents the page model for adding or editing a game list entry.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed partial class Edit : PageModel
{
    private readonly GamelistService _gamelistService;
    private readonly IgdbLookupService _igdbLookupService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="gamelistService">The game list service.</param>
    /// <param name="igdbLookupService">The IGDB lookup service.</param>
    public Edit(GamelistService gamelistService, IgdbLookupService igdbLookupService)
    {
        _gamelistService = gamelistService;
        _igdbLookupService = igdbLookupService;
    }

    /// <summary>
    ///     Gets a value indicating whether a new entry is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new entry is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the entry being edited.
    /// </summary>
    /// <value>The ID of the entry being edited, or <see langword="null" /> if a new entry is being created.</value>
    public Guid? Id { get; private set; }

    /// <summary>
    ///     Gets the URL of the game's page on IGDB.
    /// </summary>
    /// <value>The URL, or <see langword="null" /> if the game isn't linked to IGDB.</value>
    public string? IgdbUrl { get; private set; }

    /// <summary>
    ///     Gets or sets the entry being edited, if any.
    /// </summary>
    /// <value>The entry being edited, or default values if a new entry is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    [GeneratedRegex(@"^[^\s/\\?#%]+$")]
    private static partial Regex SlugPattern();

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the entry to edit. If <see langword="null" />, a new entry will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        if (!id.HasValue)
        {
            CreatingNew = true;
            Input = new EditModel { State = PlayableState.PlanToPlay };
            return Page();
        }

        var result = _gamelistService.GetPlayableById(id.Value);
        if (result.IsFailed)
        {
            return NotFound();
        }

        var playable = result.Value;
        Id = playable.Id;
        Input = new EditModel
        {
            Title = playable.Title,
            SortTitle = playable.SortTitle,
            State = playable.State,
            Igdb = playable.IgdbSlug,
            Platforms = playable.Platforms,
            Editions = playable.Editions
                .Select(edition => new EditionModel
                {
                    Label = edition.Label, Igdb = edition.IgdbSlug, Platforms = edition.Platforms
                })
                .ToList()
        };

        if (playable.IgdbSlug is { } slug)
        {
            IgdbUrl = $"https://www.igdb.com/games/{slug}";
        }

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for looking up game metadata by title.
    /// </summary>
    /// <param name="query">The title to search for.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A JSON payload of matching candidates, or an error message if none were found.</returns>
    public async Task<IActionResult> OnPostLookupAsync(string? query, CancellationToken cancellationToken)
    {
        var result = await _igdbLookupService.SearchAsync(query ?? string.Empty, cancellationToken);
        if (result.IsFailed)
        {
            return new JsonResult(new { error = result.Errors[0].Message });
        }

        var candidates = result.Value.Select(c => new { c.Title, c.Slug, c.Year, c.Type });
        return new JsonResult(new { candidates });
    }

    /// <summary>
    ///     Handles the POST request for recomputing the sort title from a title.
    /// </summary>
    /// <param name="title">The title to compute the sort title of.</param>
    /// <returns>A JSON payload of the sort title, or <see langword="null" /> if the title needs none.</returns>
    public IActionResult OnPostSortTitle(string? title)
    {
        return new JsonResult(new { sortTitle = SortTitles.Recompute(title ?? string.Empty) });
    }

    /// <summary>
    ///     Handles the POST request for saving the entry.
    /// </summary>
    /// <param name="id">The ID of the entry being edited. If <see langword="null" />, a new entry is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var valid = TryParseSlug(Input.Igdb, $"{nameof(Input)}.{nameof(Input.Igdb)}", out var slug);

        var editions = new List<EditionInput>();
        for (var index = 0; index < Input.Editions.Count; index++)
        {
            var edition = Input.Editions[index];
            var field = $"{nameof(Input)}.{nameof(Input.Editions)}[{index}].{nameof(EditionModel.Igdb)}";
            valid &= TryParseSlug(edition.Igdb, field, out var editionSlug);
            editions.Add(new EditionInput(edition.Label.Trim(), editionSlug, edition.Platforms));
        }

        if (!valid)
        {
            return Page();
        }

        var title = Input.Title.Trim();
        var sortTitle = SortTitles.Normalize(Input.SortTitle);
        var result = id is null
            ? _gamelistService.AddPlayable(title, Input.State, slug, Input.Platforms, editions, sortTitle)
            : _gamelistService.UpdatePlayable(id.Value, title, Input.State, slug, Input.Platforms, editions, sortTitle);

        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage("Index");
    }

    /// <summary>
    ///     Handles the POST request for removing the entry from the game list.
    /// </summary>
    /// <param name="id">The ID of the entry to remove.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid? id)
    {
        if (id is not { } playableId)
        {
            return BadRequest("Save the entry before it can be deleted.");
        }

        _gamelistService.DeletePlayable(playableId);
        return RedirectToPage("Index");
    }

    private bool TryParseSlug(string? reference, string field, out string? slug)
    {
        slug = null;
        if (reference?.Trim() is not { Length: > 0 } trimmed)
        {
            return true;
        }

        slug = ExtractSlug(trimmed);
        if (SlugPattern().IsMatch(slug))
        {
            return true;
        }

        ModelState.AddModelError(field, "That doesn't look like an IGDB slug or link.");
        return false;
    }

    private static string ExtractSlug(string reference)
    {
        const string marker = "/games/";

        var start = reference.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return reference.ToLowerInvariant();
        }

        var slug = reference[(start + marker.Length)..];
        var end = slug.IndexOfAny(['/', '?', '#']);
        return (end < 0 ? slug : slug[..end]).ToLowerInvariant();
    }

    /// <summary>
    ///     Represents the model for adding or editing a game list entry.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the title of the game.
        /// </summary>
        /// <value>The title of the game.</value>
        [Required]
        [StringLength(128)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the sort title, which overrides the default of filing under the title minus a leading article.
        /// </summary>
        /// <value>The sort title, or <see langword="null" /> to file it by its title.</value>
        [StringLength(128)]
        public string? SortTitle { get; set; }

        /// <summary>
        ///     Gets or sets the state of the game.
        /// </summary>
        /// <value>The state of the game.</value>
        public PlayableState State { get; set; } = PlayableState.PlanToPlay;

        /// <summary>
        ///     Gets or sets the platforms the game has been played on.
        /// </summary>
        /// <value>The platforms.</value>
        public List<GamePlatform> Platforms { get; set; } = [];

        /// <summary>
        ///     Gets or sets the additional editions of the game that have also been played.
        /// </summary>
        /// <value>The additional editions.</value>
        public List<EditionModel> Editions { get; set; } = [];

        /// <summary>
        ///     Gets or sets the IGDB game to link to: a slug or an <c>igdb.com</c> URL.
        /// </summary>
        /// <value>The reference, or <see langword="null" /> if the game shouldn't be linked to IGDB.</value>
        public string? Igdb { get; set; }
    }

    /// <summary>
    ///     Represents the model for an additional edition of a game.
    /// </summary>
    public sealed class EditionModel
    {
        /// <summary>
        ///     Gets or sets the label of the edition.
        /// </summary>
        /// <value>The label, such as <c>Director's Cut</c>.</value>
        [Required]
        [StringLength(64)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the IGDB game the edition links to: a slug or an <c>igdb.com</c> URL.
        /// </summary>
        /// <value>The reference, or <see langword="null" /> if the edition has no IGDB entry of its own.</value>
        public string? Igdb { get; set; }

        /// <summary>
        ///     Gets or sets the platforms the edition has been played on.
        /// </summary>
        /// <value>The platforms.</value>
        public List<GamePlatform> Platforms { get; set; } = [];
    }

    /// <summary>
    ///     Represents an edition's form fields, as rendered by the edition block partial.
    /// </summary>
    /// <param name="Index">The index the fields are named with, or a placeholder for the client-side template.</param>
    /// <param name="Edition">The edition.</param>
    public sealed record EditionBlock(string Index, EditionModel Edition);
}
