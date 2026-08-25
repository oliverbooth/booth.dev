using System.Xml.Serialization;

namespace BoothDotDev.Data.Models.Rss;

/// <summary>
///     Represents an item in a generic (non-blog-specific) RSS feed.
/// </summary>
public sealed class RssItem
{
    /// <summary>
    ///     Gets or sets the title of the item.
    /// </summary>
    /// <value>The title of the item.</value>
    [XmlElement("title")]
    public string Title { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the link to the item.
    /// </summary>
    /// <value>The link to the item.</value>
    [XmlElement("link")]
    public string Link { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the creator of the item.
    /// </summary>
    /// <value>The creator of the item, or <see langword="null" /> to omit the element.</value>
    [XmlElement("creator", Namespace = "http://purl.org/dc/elements/1.1/")]
    public string? Creator { get; set; }

    /// <summary>
    ///     Gets or sets the publication date of the item.
    /// </summary>
    /// <value>The publication date of the item.</value>
    [XmlElement("pubDate")]
    public string PubDate { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the GUID of the item.
    /// </summary>
    /// <value>The GUID of the item.</value>
    [XmlElement("guid")]
    public RssItemGuid Guid { get; set; } = default!;

    /// <summary>
    ///     Gets or sets the description of the item.
    /// </summary>
    /// <value>The description of the item.</value>
    [XmlElement("description")]
    public string Description { get; set; } = null!;

    /// <summary>
    ///     Determines whether <see cref="Creator" /> should be serialized - <see cref="XmlSerializer" />'s convention
    ///     for conditionally omitting an element, since most feeds (everything but Blog) have no creator concept.
    /// </summary>
    public bool ShouldSerializeCreator()
    {
        return Creator is not null;
    }
}
