using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Tutorials;

internal sealed class Index : PageModel
{
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="tutorialService">The tutorial service.</param>
    public Index(TutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    public TutorialFolder? CurrentFolder { get; private set; }

    public void OnGet([FromRoute(Name = "slug")] string? slug)
    {
        if (slug is null)
        {
            return;
        }

        var tokens = slug.Split('/');
        TutorialFolder? folder = null;

        foreach (var token in tokens)
        {
            folder = _tutorialService.GetFolder(token, folder);
        }

        CurrentFolder = folder;
    }
}
