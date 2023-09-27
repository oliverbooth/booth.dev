using System.Xml.Serialization;

namespace OliverBooth.Data.Blog.Rss;

[XmlRoot("rss")]
public sealed class BlogRoot
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "2.0";

    [XmlElement("channel")]
    public BlogChannel Channel { get; set; } = default!;
}
