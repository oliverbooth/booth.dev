using System.Xml.Serialization;

namespace BoothDotDev.Data.Blog.Rss;

/// <summary>
///     Represents the GUID of a blog item in an RSS feed.
/// </summary>
public struct BlogItemGuid
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogItemGuid" /> structure.
    /// </summary>
    public BlogItemGuid()
    {
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the GUID is a permanent link.
    /// </summary>
    /// <value><see langword="true" /> if the GUID is a permanent link; otherwise, <see langword="false" />.</value>
    [XmlAttribute("isPermaLink")]
    public bool IsPermaLink { get; set; } = false;

    /// <summary>
    ///     Gets or sets the value of the GUID.
    /// </summary>
    /// <value>The value of the GUID.</value>
    [XmlText]
    public string Value { get; set; } = null!;

    /// <summary>
    ///     Implicitly converts a string to a <see cref="BlogItemGuid" />.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A <see cref="BlogItemGuid" /> with the specified value.</returns>
    public static implicit operator BlogItemGuid(string value) => new() { Value = value };
}
