namespace OliverBooth.Common.Data.Web;

/// <summary>
///     Represents a folder for tutorial articles.
/// </summary>
public interface ITutorialFolder
{
    /// <summary>
    ///     Gets the ID of this folder.
    /// </summary>
    /// <value>The ID of the folder.</value>
    int Id { get; }

    /// <summary>
    ///     Gets the ID of this folder's parent.
    /// </summary>
    /// <value>The ID of the parent, or <see langword="null" /> if this folder is at the root.</value>
    int? Parent { get; }

    /// <summary>
    ///     Gets the URL of the folder's preview image.
    /// </summary>
    /// <value>The preview image URL.</value>
    Uri? PreviewImageUrl { get; }

    /// <summary>
    ///     Gets the slug of this folder.
    /// </summary>
    /// <value>The slug.</value>
    string Slug { get; }

    /// <summary>
    ///     Gets the title of this folder.
    /// </summary>
    /// <value>The title.</value>
    string Title { get; }

    /// <summary>
    ///     Gets the visibility of this article.
    /// </summary>
    /// <value>The visibility of the article.</value>
    Visibility Visibility { get; }
}
