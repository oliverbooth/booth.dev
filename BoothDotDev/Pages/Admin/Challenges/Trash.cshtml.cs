using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using DEDrake;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Challenges;

using DevChallenge = DevChallenge;

/// <summary>
///     Represents the page model for the admin challenge trash page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Trash : PageModel
{
    private readonly DevChallengeService _devChallengeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="devChallengeService">The <see cref="DevChallengeService" />.</param>
    public Trash(DevChallengeService devChallengeService)
    {
        _devChallengeService = devChallengeService;
    }

    /// <summary>
    ///     Gets the list of trashed challenges, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed challenges.</value>
    public IReadOnlyList<DevChallenge> Challenges { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Challenges = _devChallengeService.GetTrashedChallenges();
    }

    /// <summary>
    ///     Handles the POST request for restoring a trashed challenge.
    /// </summary>
    /// <param name="id">The ID of the challenge to restore.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostRestore(string id)
    {
        _devChallengeService.RestoreChallenge(ShortGuid.Parse(id));
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting a single trashed challenge.
    /// </summary>
    /// <param name="id">The ID of the challenge to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDelete(string id)
    {
        _devChallengeService.PermanentlyDeleteChallenge(ShortGuid.Parse(id));
        return RedirectToPage();
    }

    /// <summary>
    ///     Handles the POST request for permanently deleting every selected trashed challenge.
    /// </summary>
    /// <param name="ids">The IDs of the challenges to permanently delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostPermanentlyDeleteBulk(List<string> ids)
    {
        foreach (var id in ids)
        {
            _devChallengeService.PermanentlyDeleteChallenge(ShortGuid.Parse(id));
        }

        return RedirectToPage();
    }
}
