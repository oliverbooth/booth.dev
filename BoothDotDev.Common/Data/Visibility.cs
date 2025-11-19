using NpgsqlTypes;

namespace BoothDotDev.Common.Data;

/// <summary>
///     An enumeration of the possible visibilities of a blog post.
/// </summary>
public enum Visibility
{
    /// <summary>
    ///     Used for filtering results. Represents all visibilities.
    /// </summary>
    None = -1,

    /// <summary>
    ///     The post is private and only visible to the author, or those with the password.
    /// </summary>
    [PgName("private")]
    Private,

    /// <summary>
    ///     The post is unlisted and only visible to those with the link.
    /// </summary>
    [PgName("unlisted")]
    Unlisted,

    /// <summary>
    ///     The post is published and visible to everyone.
    /// </summary>
    [PgName("published")]
    Published
}
