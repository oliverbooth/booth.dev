using System.Xml.Serialization;

namespace BoothDotDev.Data.Blog.Rss;

public sealed class BlogChannel
{
    [XmlElement("title")]
    public string Title { get; set; } = default!;

    [XmlElement("link", Namespace = "http://www.w3.org/2005/Atom")]
    public AtomLink AtomLink { get; set; } = default!;

    [XmlElement("link")]
    public string Link { get; set; } = default!;

    [XmlElement("description")]
    public string Description { get; set; } = default!;

    [XmlElement("lastBuildDate")]
    public string LastBuildDate { get; set; } = default!;

    [XmlElement("updatePeriod", Namespace = "http://purl.org/rss/1.0/modules/syndication/")]
    public string UpdatePeriod { get; set; } = "hourly";

    [XmlElement("updateFrequency", Namespace = "http://purl.org/rss/1.0/modules/syndication/")]
    public string UpdateFrequency { get; set; } = "1";

    [XmlElement("generator")]
    public string Generator { get; set; } = default!;

    [XmlElement("item")]
    public List<BlogItem> Items { get; set; } = new();
}