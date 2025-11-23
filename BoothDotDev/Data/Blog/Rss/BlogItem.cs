using System.Xml.Serialization;

namespace BoothDotDev.Data.Blog.Rss;

/// <summary>
///     Represents an item in an RSS feed.
/// </summary>
public sealed class BlogItem
{
    /// <summary>
    ///     Gets or sets the title of the blog item.
    /// </summary>
    /// <value>The title of the blog item.</value>
    [XmlElement("title")]
    public string Title { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the link to the blog item.
    /// </summary>
    /// <value>The link to the blog item.</value>
    [XmlElement("link")]
    public string Link { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the comments link for the blog item.
    /// </summary>
    /// <value>The comments link for the blog item.</value>
    [XmlElement("comments")]
    public string Comments { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the creator of the blog item.
    /// </summary>
    /// <value>The creator of the blog item.</value>
    [XmlElement("creator", Namespace = "http://purl.org/dc/elements/1.1/")]
    public string Creator { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the publication date of the blog item.
    /// </summary>
    /// <value>The publication date of the blog item.</value>
    [XmlElement("pubDate")]
    public string PubDate { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the GUID of the blog item.
    /// </summary>
    /// <value>The GUID of the blog item.</value>
    [XmlElement("guid")]
    public BlogItemGuid Guid { get; set; } = default!;

    /// <summary>
    ///     Gets or sets the description of the blog item.
    /// </summary>
    /// <value>The description of the blog item.</value>
    [XmlElement("description")]
    public string Description { get; set; } = null!;
}
