namespace BoothDotDev.Markdown;

/// <summary>
///     Represents the context for rendering markdown content, including its unique identifier and date.
/// </summary>
public readonly struct MarkdownRenderContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MarkdownRenderContext" /> struct.
    /// </summary>
    /// <param name="id">The unique identifier of the markdown content.</param>
    /// <param name="date">The date and time of the markdown content.</param>
    public MarkdownRenderContext(Guid id, DateTimeOffset date)
    {
        Id = id;
        Date = date;
    }

    /// <summary>
    ///     Gets or sets the unique identifier of the markdown content.
    /// </summary>
    /// <value>The unique identifier of the markdown content.</value>
    public Guid Id { get; }

    /// <summary>
    ///     Gets or sets the date and time of the markdown content.
    /// </summary>
    /// <value>The date and time of the markdown content.</value>
    public DateTimeOffset Date { get; }
}
