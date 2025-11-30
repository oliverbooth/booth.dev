namespace BoothDotDev.Data;

/// <summary>
///     Represents an item in a table of contents.
/// </summary>
public class TocItem
{
    /// <summary>
    ///     Gets the child items of the table of contents item.
    /// </summary>
    /// <value>The child items of the table of contents item.</value>
    public List<TocItem> Children { get; } = [];

    /// <summary>
    ///     Gets or sets the level of the table of contents item.
    /// </summary>
    /// <value>The level of the table of contents item.</value>
    public int Level { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the table of contents item.
    /// </summary>
    /// <value>The ID of the table of contents item.</value>
    public string Id { get; set; } = "";


    /// <summary>
    ///     Gets or sets the text of the table of contents item.
    /// </summary>
    /// <value>The text of the table of contents item.</value>
    public string Text { get; set; } = "";
}
