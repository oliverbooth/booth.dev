using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Gamelist;

/// <summary>
///     Represents the page model for the admin game list page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    /// <summary>
    ///     The display order, label, and dot colour for each state's group.
    /// </summary>
    private static readonly (PlayableState State, string Label, string DotClass)[] StateOrder =
    [
        (PlayableState.Playing, "playing", "dot"),
        (PlayableState.PlanToPlay, "plan to play", "dot dot-tangerine"),
        (PlayableState.Played, "played", "dot dot-bubblegum")
    ];

    private readonly GamelistService _gamelistService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="gamelistService">The game list service.</param>
    public Index(GamelistService gamelistService)
    {
        _gamelistService = gamelistService;
    }

    /// <summary>
    ///     Gets the games, grouped by state, in <see cref="StateOrder" />.
    /// </summary>
    /// <value>The state groups.</value>
    public IReadOnlyList<StateGroup> StateGroups { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        var playables = _gamelistService.GetAllPlayables();
        StateGroups = StateOrder
            .Select(s => new StateGroup(s.State, s.Label, s.DotClass, playables.Where(p => p.State == s.State).ToArray()))
            .ToArray();
    }

    /// <summary>
    ///     Handles the POST request for changing a game's state.
    /// </summary>
    /// <param name="id">The ID of the game to update.</param>
    /// <param name="state">The new state.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostSetState(Guid id, PlayableState state)
    {
        _gamelistService.SetState(id, state);
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for removing a game from the game list.
    /// </summary>
    /// <param name="id">The ID of the game to remove.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        _gamelistService.DeletePlayable(id);
        return RedirectToPage();
    }

    /// <summary>
    ///     Represents a group of games sharing a state, for display in the admin listing.
    /// </summary>
    /// <param name="State">The state shared by every game in the group.</param>
    /// <param name="Label">The lowercase display label for the state.</param>
    /// <param name="DotClass">The CSS class for this state's indicator dot.</param>
    /// <param name="Playables">The games in this state.</param>
    public sealed record StateGroup(PlayableState State, string Label, string DotClass, IReadOnlyList<Playable> Playables);
}
