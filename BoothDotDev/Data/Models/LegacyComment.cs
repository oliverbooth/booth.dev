using System.Web;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a comment that was posted on a legacy comment framework.
/// </summary>
public sealed class LegacyComment
{
    /// <summary>
    ///     Gets the PNG-encoded avatar of the author.
    /// </summary>
    /// <value>The author's avatar.</value>
    public string? Avatar { get; set; }

    /// <summary>
    ///     Gets the name of the comment's author.
    /// </summary>
    /// <value>The author's name.</value>
    public string Author { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the body of the comment.
    /// </summary>
    /// <value>The comment body.</value>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the date and time at which this comment was posted.
    /// </summary>
    /// <value>The creation timestamp.</value>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the ID of this comment.
    /// </summary>
    /// <value>The comment ID.</value>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Gets the ID of the parent comment, if this comment is a reply.
    /// </summary>
    /// <value>The parent comment ID.</value>
    public Guid? ParentComment { get; private set; }

    /// <summary>
    ///     Gets the ID of the post to which this comment was posted.
    /// </summary>
    /// <value>The post ID.</value>
    public Guid PostId { get; private set; }

    /// <summary>
    ///     Gets the avatar URL of the comment's author.
    /// </summary>
    /// <returns>The avatar URL.</returns>
    public string GetAvatarUrl()
    {
        return Avatar ?? $"https://ui-avatars.com/api/?name={HttpUtility.UrlEncode(Author)}";
    }
}
