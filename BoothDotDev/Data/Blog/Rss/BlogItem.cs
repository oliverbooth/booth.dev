using System.Xml.Serialization;

namespace BoothDotDev.Data.Blog.Rss;

public sealed class BlogItem
{
    [XmlElement("title")]
    public string Title { get; set; } = default!;

    [XmlElement("link")]
    public string Link { get; set; } = default!;

    [XmlElement("comments")]
    public string Comments { get; set; } = default!;

    [XmlElement("creator", Namespace = "http://purl.org/dc/elements/1.1/")]
    public string Creator { get; set; } = default!;

    [XmlElement("pubDate")]
    public string PubDate { get; set; } = default!;

    [XmlElement("guid")]
    public BlogItemGuid Guid { get; set; } = default!;

    [XmlElement("description")]
    public string Description { get; set; } = default!;
}
