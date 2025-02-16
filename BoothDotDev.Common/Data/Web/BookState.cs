namespace BoothDotDev.Common.Data.Web;

/// <summary>
///     Represents the state of a book.
/// </summary>
public enum BookState
{
    /// <summary>
    ///     The book has been read and finished.
    /// </summary>
    Read,

    /// <summary>
    ///     The book is on the current reading list.
    /// </summary>
    Reading,

    /// <summary>
    ///     The book is on a future reading list.
    /// </summary>
    PlanToRead
}