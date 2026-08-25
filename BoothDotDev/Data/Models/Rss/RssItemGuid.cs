using System.Xml.Serialization;

namespace BoothDotDev.Data.Models.Rss;

/// <summary>
///     Represents the GUID of an item in an RSS feed.
/// </summary>
public struct RssItemGuid
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RssItemGuid" /> structure.
    /// </summary>
    public RssItemGuid()
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
    ///     Implicitly converts a string to a <see cref="RssItemGuid" />.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A <see cref="RssItemGuid" /> with the specified value.</returns>
    public static implicit operator RssItemGuid(string value)
    {
        return new RssItemGuid { Value = value };
    }
}
