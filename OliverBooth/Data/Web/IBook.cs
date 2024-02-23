namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a book.
/// </summary>
public interface IBook
{
    /// <summary>
    ///     Gets the author of the book.
    /// </summary>
    /// <value>The author of the book.</value>
    string Author { get; }

    /// <summary>
    ///     Gets the ISBN of the book.
    /// </summary>
    /// <value>The ISBN of the book.</value>
    string Isbn { get; }

    /// <summary>
    ///     Gets the state of the book.
    /// </summary>
    /// <value>The state of the book.</value>
    BookState State { get; }

    /// <summary>
    ///     Gets the title of the book.
    /// </summary>
    /// <value>The title of the book.</value>
    string Title { get; }

    /// <summary>
    ///     Generates the barcode for this book.
    /// </summary>
    /// <returns>The EAN-13 barcode encoded as PNG in Base64.</returns>
    /// <remarks>This value should be disposed.</remarks>
    string GetBarcode();
}