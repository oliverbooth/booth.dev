namespace BoothDotDev.Data;

public class TocItem
{
    public string Text { get; set; } = "";
    public int Level { get; set; }
    public string Id { get; set; } = "";
    public List<TocItem> Children { get; } = new List<TocItem>();
}