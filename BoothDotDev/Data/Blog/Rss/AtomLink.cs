using System.Xml.Serialization;

namespace BoothDotDev.Data.Blog.Rss;

/// <summary>
///     Represents an Atom link in an RSS feed.
/// </summary>
public sealed class AtomLink
{
    /// <summary>
    ///     Gets or sets the href of the Atom link.
    /// </summary>
    /// <value>The href of the Atom link.</value>
    [XmlAttribute("href")]
    public string Href { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the rel of the Atom link.
    /// </summary>
    /// <value>The rel of the Atom link.</value>
    [XmlAttribute("rel")]
    public string Rel { get; set; } = "self";

    /// <summary>
    ///     Gets or sets the type of the Atom link.
    /// </summary>
    /// <value>The type of the Atom link.</value>
    [XmlAttribute("type")]
    public string Type { get; set; } = "application/rss+xml";
}
