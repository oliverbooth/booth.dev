namespace OliverBooth.Common.Data.Blog;

/// <summary>
///     Represents a comment that was posted on a legacy comment framework.
/// </summary>
public interface ILegacyComment
{
    /// <summary>
    ///     Gets the PNG-encoded avatar of the author.
    /// </summary>
    /// <value>The author's avatar.</value>
    string? Avatar { get; }

    /// <summary>
    ///     Gets the name of the comment's author.
    /// </summary>
    /// <value>The author's name.</value>
    string Author { get; }

    /// <summary>
    ///     Gets the body of the comment.
    /// </summary>
    /// <value>The comment body.</value>
    string Body { get; }

    /// <summary>
    ///     Gets the date and time at which this comment was posted.
    /// </summary>
    /// <value>The creation timestamp.</value>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    ///     Gets the ID of this comment.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     Gets the ID of the comment this comment is replying to.
    /// </summary>
    /// <value>The parent comment ID, or <see langword="null" /> if this comment is not a reply.</value>
    Guid? ParentComment { get; }

    /// <summary>
    ///     Gets the ID of the post to which this comment was posted.
    /// </summary>
    /// <value>The post ID.</value>
    Guid PostId { get; }

    /// <summary>
    ///     Gets the avatar URL of the comment's author.
    /// </summary>
    /// <returns>The avatar URL.</returns>
    string GetAvatarUrl();
}
