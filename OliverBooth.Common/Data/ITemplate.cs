namespace OliverBooth.Common.Data;

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
}
