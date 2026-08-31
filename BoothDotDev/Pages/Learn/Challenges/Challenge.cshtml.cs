using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Learn.Challenges;

internal sealed class Challenge : PageModel
{
    private readonly DevChallengeService _devChallengeService;

    public Challenge(DevChallengeService devChallengeService)
    {
        _devChallengeService = devChallengeService;
    }

    public DevChallenge DevChallenge { get; private set; } = null!;

    public IActionResult OnGet([FromRoute] string id)
    {
        if (!_devChallengeService.TryGetDevChallenge(id, out var challenge, out var shouldRedirect))
        {
            return NotFound();
        }

        // There's no legitimate reason for a signed-out visitor to reach a private challenge by its public
        // URL - the editor's preview pane covers previewing unpublished work.
        if (challenge.Visibility == Visibility.Private && User.Identity?.IsAuthenticated != true)
        {
            return NotFound();
        }

        if (shouldRedirect)
        {
            return RedirectToPagePermanent("/Learn/Challenges/Challenge", new { id = challenge.Id });
        }

        DevChallenge = challenge;
        return Page();
    }
}
