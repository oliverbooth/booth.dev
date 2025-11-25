using BoothDotDev.Common.Data.Web;

namespace BoothDotDev.Data.Web;

/// <summary>
///     Represents a book.
/// </summary>
internal sealed class Book : IBook
{
    /// <inheritdoc />
    public string Author { get; private set; } = string.Empty;

    /// <inheritdoc />
    public string Isbn { get; private set; } = string.Empty;

    /// <inheritdoc />
    public BookState State { get; private set; }

    /// <inheritdoc />
    public string Title { get; private set; } = string.Empty;
}
