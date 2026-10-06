namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a book.
/// </summary>
public sealed class Book
{
    /// <summary>
    ///     Gets or sets the author of the book.
    /// </summary>
    /// <value>The author of the book.</value>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the ISBN of the book.
    /// </summary>
    /// <value>The ISBN of the book.</value>
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the state of the book.
    /// </summary>
    /// <value>The state of the book.</value>
    public BookState State { get; set; }

    /// <summary>
    ///     Gets or sets the title of the book.
    /// </summary>
    /// <value>The title of the book.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the sort title of the book, which overrides the default of filing under the title minus a leading article.
    /// </summary>
    /// <value>The sort title, or <see langword="null" /> to file the book by its title.</value>
    public string? SortTitle { get; set; }

    /// <summary>
    ///     Gets the name the book files under in lists.
    /// </summary>
    /// <value>The sort title, or the title minus a leading "The", "A" or "An" if there isn't one.</value>
    public string SortKey
    {
        get => SortTitle ?? SortTitles.Default(Title);
    }
}
