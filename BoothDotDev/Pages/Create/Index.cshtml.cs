using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Create;

/// <summary>
///     Renders the Create page.
/// </summary>
public sealed class Index : PageModel
{
    private readonly CreationService _creationService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Index" /> class.
    /// </summary>
    /// <param name="creationService">The creation service.</param>
    public Index(CreationService creationService)
    {
        _creationService = creationService;
    }

    /// <summary>
    ///     Gets the artwork items.
    /// </summary>
    /// <value>The artwork items.</value>
    public IReadOnlyList<ArtworkItem> ArtworkItems { get; set; } = [];

    /// <summary>
    ///     Handles the HTTP GET request.
    /// </summary>
    public void OnGet()
    {
        ArtworkItems = _creationService.GetArtworkItems();
    }
}
