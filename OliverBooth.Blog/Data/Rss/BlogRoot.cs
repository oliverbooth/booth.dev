using System.Xml.Serialization;

namespace OliverBooth.Data.Rss;

[XmlRoot("rss")]
public sealed class BlogRoot
{
    [XmlAttribute("version")]
    public string Version { get; set; } = default!;

    [XmlElement("channel")]
    public BlogChannel Channel { get; set; } = default!;
}
