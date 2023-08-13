namespace OliverBooth.Data.Blog;

/// <summary>
///     Represents the author of a blog post.
/// </summary>
public interface IBlogAuthor
{
    /// <summary>
    ///     Gets the URL of the author's avatar.
    /// </summary>
    /// <value>The URL of the author's avatar.</value>
    Uri AvatarUrl { get; }

    /// <summary>
    ///     Gets the display name of the author.
    /// </summary>
    /// <value>The display name of the author.</value>
    string DisplayName { get; }

    /// <summary>
    ///     Gets the unique identifier of the author.
    /// </summary>
    /// <value>The unique identifier of the author.</value>
    Guid Id { get; }

    /// <summary>
    ///     Gets the URL of the author's avatar.
    /// </summary>
    /// <param name="size">The size of the avatar.</param>
    /// <returns>The URL of the author's avatar.</returns>
    Uri GetAvatarUrl(int size = 28);
}
