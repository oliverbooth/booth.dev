using BoothDotDev.Data.Models;
using BoothDotDev.Services;

namespace BoothDotDev.Pages.Admin.Tutorials;

/// <summary>
///     Represents the page model for the admin tutorial trash page.
/// </summary>
public sealed class Trash : TrashPageModel<TutorialArticle>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Trash" /> class.
    /// </summary>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    public Trash(TutorialService tutorialService)
        : base(tutorialService.GetTrashedArticles, id => tutorialService.RestoreArticle(id), id => tutorialService.PermanentlyDeleteArticle(id))
    {
    }

    /// <summary>
    ///     Gets the list of trashed articles, newest-trashed first.
    /// </summary>
    /// <value>The list of trashed articles.</value>
    public IReadOnlyList<TutorialArticle> Articles => Items;
}
