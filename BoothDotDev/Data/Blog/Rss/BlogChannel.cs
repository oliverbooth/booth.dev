using System.Xml.Serialization;

namespace BoothDotDev.Data.Blog.Rss;

/// <summary>
///     Represents the channel of an RSS feed.
/// </summary>
public sealed class BlogChannel
{
    /// <summary>
    ///     Gets or sets the title of the RSS feed.
    /// </summary>
    /// <value>The title of the RSS feed.</value>
    [XmlElement("title")]
    public string Title { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the Atom link of the RSS feed.
    /// </summary>
    /// <value>The Atom link of the RSS feed.</value>
    [XmlElement("link", Namespace = "http://www.w3.org/2005/Atom")]
    public AtomLink AtomLink { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the link of the RSS feed.
    /// </summary>
    /// <value>The link of the RSS feed.</value>
    [XmlElement("link")]
    public string Link { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the description of the RSS feed.
    /// </summary>
    /// <value>The description of the RSS feed.</value>
    [XmlElement("description")]
    public string Description { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the last build date of the RSS feed.
    /// </summary>
    /// <value>The last build date of the RSS feed.</value>
    [XmlElement("lastBuildDate")]
    public string LastBuildDate { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the update period of the RSS feed.
    /// </summary>
    /// <value>The update period of the RSS feed.</value>
    [XmlElement("updatePeriod", Namespace = "http://purl.org/rss/1.0/modules/syndication/")]
    public string UpdatePeriod { get; set; } = "hourly";

    /// <summary>
    ///     Gets or sets the update frequency of the RSS feed.
    /// </summary>
    /// <value>The update frequency of the RSS feed.</value>
    [XmlElement("updateFrequency", Namespace = "http://purl.org/rss/1.0/modules/syndication/")]
    public string UpdateFrequency { get; set; } = "1";

    /// <summary>
    ///     Gets or sets the generator of the RSS feed.
    /// </summary>
    /// <value>The generator of the RSS feed.</value>
    [XmlElement("generator")]
    public string Generator { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the items of the RSS feed.
    /// </summary>
    /// <value>The items of the RSS feed.</value>
    [XmlElement("item")]
    public List<BlogItem> Items { get; set; } = [];
}
