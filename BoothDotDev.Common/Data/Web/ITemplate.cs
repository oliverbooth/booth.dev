namespace BoothDotDev.Common.Data.Web;

/// <summary>
///     Represents a template.
/// </summary>
public interface ITemplate
{
    /// <summary>
    ///     Gets or sets the format string.
    /// </summary>
    /// <value>The format string.</value>
    string FormatString { get; }

    /// <summary>
    ///     Gets the name of the template.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the variant of the template.
    /// </summary>
    /// <value>The variant of the template.</value>
    string Variant { get; }
}
