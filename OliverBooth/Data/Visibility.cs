namespace OliverBooth.Data;

/// <summary>
///     An enumeration of the possible visibilities of a blog post.
/// </summary>
public enum Visibility
{
    /// <summary>
    ///     The post is private and only visible to the author, or those with the password.
    /// </summary>
    Private,

    /// <summary>
    ///     The post is unlisted and only visible to those with the link.
    /// </summary>
    Unlisted,

    /// <summary>
    ///     The post is published and visible to everyone.
    /// </summary>
    Published
}
