using System.Xml.Serialization;

namespace BoothDotDev.Data.Blog.Rss;

/// <summary>
///     Represents the root of an RSS feed.
/// </summary>
[XmlRoot("rss")]
public sealed class BlogRoot
{
    /// <summary>
    ///     Gets or sets the RSS version.
    /// </summary>
    /// <value>The RSS version.</value>
    [XmlAttribute("version")]
    public string Version { get; set; } = "2.0";

    /// <summary>
    ///     Gets or sets the channel of the RSS feed.
    /// </summary>
    /// <value>The channel of the RSS feed.</value>
    [XmlElement("channel")]
    public BlogChannel Channel { get; set; } = null!;
}
