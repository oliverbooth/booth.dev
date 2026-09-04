using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using DEDrake;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Challenges;

using DevChallenge = DevChallenge;

/// <summary>
///     Represents the page model for the admin challenges page.
/// </summary>
[Authorize(Policy = "Admin")]
public sealed class Index : PageModel
{
    private readonly DevChallengeService _devChallengeService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="devChallengeService">The <see cref="DevChallengeService" />.</param>
    public Index(DevChallengeService devChallengeService)
    {
        _devChallengeService = devChallengeService;
    }

    /// <summary>
    ///     Gets the list of challenges.
    /// </summary>
    /// <value>The list of challenges.</value>
    public IReadOnlyList<DevChallenge> Challenges { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    public void OnGet()
    {
        Challenges = _devChallengeService.GetAllChallenges(Visibility.None);
    }

    /// <summary>
    ///     Handles the POST request for moving a challenge to the trash.
    /// </summary>
    /// <param name="id">The ID of the challenge to trash.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(string id)
    {
        _devChallengeService.TrashChallenge(ShortGuid.Parse(id));
        return RedirectToPage();
    }
}
