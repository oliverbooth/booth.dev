namespace BoothDotDev.Markdown.Template;

/// <summary>
///     Represents the data made available to a template partial view during rendering.
/// </summary>
public sealed class TemplateModel
{
    /// <summary>
    ///     Gets or initializes the positional argument list parsed from the template invocation.
    /// </summary>
    /// <value>The positional argument list parsed from the template invocation.</value>
    public required IReadOnlyList<string> ArgumentList { get; init; }

    /// <summary>
    ///     Gets or initializes the raw, unparsed argument string from the template invocation.
    /// </summary>
    /// <value>The raw, unparsed argument string from the template invocation.</value>
    public required string ArgumentString { get; init; }

    /// <summary>
    ///     Gets or initializes any named parameters parsed from the template invocation.
    /// </summary>
    /// <value>Any named parameters parsed from the template invocation.</value>
    public required IReadOnlyDictionary<string, string> Params { get; init; }

    /// <summary>
    ///     Gets or initializes a random integer, generated fresh for this render.
    /// </summary>
    /// <value>A random integer, generated fresh for this render.</value>
    public required int RandomInt { get; init; }

    /// <summary>
    ///     Gets or initializes a random GUID (as a 32-digit hex string), generated fresh for this render.
    /// </summary>
    /// <value>The random GUID (as a 32-digit hex string), generated fresh for this render.</value>
    public required string RandomGuid { get; init; }

    /// <summary>
    ///     Gets or initializes the variant of the template to render.
    /// </summary>
    /// <value>The variant of the template to render.</value>
    public string? Variant { get; init; }
}
