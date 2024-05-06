namespace OliverBooth.Common.Services;

/// <summary>
///     Represents a service which can perform programming language lookup.
/// </summary>
public interface IProgrammingLanguageService
{
    /// <summary>
    ///     Returns the human-readable name of a language.
    /// </summary>
    /// <param name="alias">The alias of the language.</param>
    /// <returns>The human-readable name, or <paramref name="alias" /> if the name could not be found.</returns>
    string GetLanguageName(string alias);
}
