namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a book.
/// </summary>
internal sealed class Book : IBook
{
    /// <inheritdoc />
    public string Author { get; }

    /// <inheritdoc />
    public string Isbn { get; }

    /// <inheritdoc />
    public BookState State { get; }

    /// <inheritdoc />
    public string Title { get; }
}
