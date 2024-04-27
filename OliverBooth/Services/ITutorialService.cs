using System.Diagnostics.CodeAnalysis;
using OliverBooth.Data;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service which can retrieve tutorial articles.
/// </summary>
public interface ITutorialService
{
    /// <summary>
    ///     Gets the articles within a tutorial folder.
    /// </summary>
    /// <param name="folder">The folder whose articles to retrieve.</param>
    /// <param name="visibility">The visibility to filter by. -1 does not filter.</param>
    /// <returns>A read-only view of the articles in the folder.</returns>
    IReadOnlyCollection<ITutorialArticle> GetArticles(ITutorialFolder folder, Visibility visibility = Visibility.None);

    /// <summary>
    ///     Gets the tutorial folders within a specified folder.
    /// </summary>
    /// <param name="parent">The parent folder.</param>
    /// <param name="visibility">The visibility to filter by. -1 does not filter.</param>
    /// <returns>A read-only view of the subfolders in the folder.</returns>
    IReadOnlyCollection<ITutorialFolder> GetFolders(ITutorialFolder? parent = null, Visibility visibility = Visibility.None);

    /// <summary>
    ///     Gets a folder by its ID.
    /// </summary>
    /// <param name="id">The ID of the folder to get</param>
    /// <param name="folder">
    ///     When this method returns, contains the folder whose ID is equal to the ID specified, or
    ///     <see langword="null" /> if no such folder was found.
    /// </param>
    /// <returns><see langword="true" /></returns> 
    ITutorialFolder? GetFolder(int id);

    /// <summary>
    ///     Gets a folder by its slug.
    /// </summary>
    /// <param name="slug">The slug of the folder.</param>
    /// <param name="parent">The parent folder.</param>
    /// <returns>The folder.</returns>
    ITutorialFolder? GetFolder(string? slug, ITutorialFolder? parent = null);

    /// <summary>
    ///     Gets the full slug of the specified folder.
    /// </summary>
    /// <param name="folder">The folder whose slug to return.</param>
    /// <returns>The full slug of the folder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="folder" /> is <see langword="null" />.</exception>
    string GetFullSlug(ITutorialFolder folder);

    /// <summary>
    ///     Gets the full slug of the specified article.
    /// </summary>
    /// <param name="article">The article whose slug to return.</param>
    /// <returns>The full slug of the article.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="article" /> is <see langword="null" />.</exception>
    string GetFullSlug(ITutorialArticle article);

    /// <summary>
    ///     Renders the body of the specified article.
    /// </summary>
    /// <param name="article">The article to render.</param>
    /// <returns>The rendered HTML of the article.</returns>
    string RenderArticle(ITutorialArticle article);

    /// <summary>
    ///     Renders the excerpt of the specified article.
    /// </summary>
    /// <param name="article">The article whose excerpt to render.</param>
    /// <param name="wasTrimmed">
    ///     When this method returns, contains <see langword="true" /> if the excerpt was trimmed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>The rendered HTML of the article's excerpt.</returns>
    string RenderExcerpt(ITutorialArticle article, out bool wasTrimmed);

    /// <summary>
    ///     Attempts to find an article by its ID.
    /// </summary>
    /// <param name="id">The ID of the article.</param>
    /// <param name="article">
    ///     When this method returns, contains the article whose ID matches the specified <paramref name="id" />, or
    ///     <see langword="null" /> if no such article was found.
    /// </param>
    /// <returns><see langword="true" /> if a matching article was found; otherwise, <see langword="false" />.</returns>
    bool TryGetArticle(int id, [NotNullWhen(true)] out ITutorialArticle? article);

    /// <summary>
    ///     Attempts to find an article by its slug.
    /// </summary>
    /// <param name="slug">The slug of the article.</param>
    /// <param name="article">
    ///     When this method returns, contains the article whose slug matches the specified <paramref name="slug" />, or
    ///     <see langword="null" /> if no such article was found.
    /// </param>
    /// <returns><see langword="true" /> if a matching article was found; otherwise, <see langword="false" />.</returns>
    bool TryGetArticle(string slug, [NotNullWhen(true)] out ITutorialArticle? article);
}