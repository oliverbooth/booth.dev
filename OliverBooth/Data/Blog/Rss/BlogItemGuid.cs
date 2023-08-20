using System.Xml.Serialization;

namespace OliverBooth.Data.Blog.Rss;

public struct BlogItemGuid
{
    public BlogItemGuid()
    {
    }

    [XmlAttribute("isPermaLink")]
    public bool IsPermaLink { get; set; } = false;

    [XmlText]
    public string Value { get; set; } = default!;

    public static implicit operator BlogItemGuid(string value) => new() { Value = value };
}
