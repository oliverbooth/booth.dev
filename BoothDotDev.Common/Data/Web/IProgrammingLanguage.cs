namespace BoothDotDev.Common.Data.Web;

/// <summary>
///     Represents a programming language.
/// </summary>
public interface IProgrammingLanguage
{
    /// <summary>
    ///     Gets the unique key for this programming language.
    /// </summary>
    /// <value>The unique key.</value>
    /// <remarks>This is generally the file extension of the language.</remarks>
    string Key { get; }

    /// <summary>
    ///     Gets the name of this programming language.
    /// </summary>
    /// <value>The name.</value>
    string Name { get; }
}
