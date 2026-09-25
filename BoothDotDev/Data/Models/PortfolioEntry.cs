namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a place in the portfolio: a project or a creation that is listed on the portfolio page, and where.
/// </summary>
/// <remarks>
///     An entry stands for exactly one of <see cref="Project" /> or <see cref="Creation" />. A project or creation with no
///     entry is not listed, though it can still be reached directly.
/// </remarks>
public sealed class PortfolioEntry
{
    /// <summary>
    ///     Gets the unique identifier for the entry.
    /// </summary>
    /// <value>The unique identifier.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets or sets the position of the entry on the portfolio page, counting from zero.
    /// </summary>
    /// <value>The position.</value>
    public int Position { get; set; }

    /// <summary>
    ///     Gets or sets the position of the entry among the featured entries shown on the home page, counting from zero.
    /// </summary>
    /// <value>The featured position, or <see langword="null" /> if the entry is not featured.</value>
    public int? FeaturedPosition { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the listed project.
    /// </summary>
    /// <value>The project ID, or <see langword="null" /> if the entry is a creation.</value>
    public Guid? ProjectId { get; set; }

    /// <summary>
    ///     Gets or sets the listed project.
    /// </summary>
    /// <value>The project, or <see langword="null" /> if the entry is a creation.</value>
    public Project? Project { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the listed creation.
    /// </summary>
    /// <value>The creation ID, or <see langword="null" /> if the entry is a project.</value>
    public Guid? CreationId { get; set; }

    /// <summary>
    ///     Gets or sets the listed creation.
    /// </summary>
    /// <value>The creation, or <see langword="null" /> if the entry is a project.</value>
    public Creation? Creation { get; set; }
}
