using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Tutorials;

internal sealed class Index : PageModel
{
    private readonly ITutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="tutorialService">The tutorial service.</param>
    public Index(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    public ITutorialFolder? CurrentFolder { get; private set; }

    public void OnGet([FromRoute(Name = "slug")] string? slug)
    {
        if (slug is null) return;

        string[] tokens = slug.Split('/');
        ITutorialFolder? folder = null;

        foreach (string token in tokens)
        {
            folder = _tutorialService.GetFolder(token, folder);
        }

        CurrentFolder = folder;
    }
}
