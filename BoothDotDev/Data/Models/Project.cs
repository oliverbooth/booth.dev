namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a project.
/// </summary>
public sealed class Project : IEquatable<Project>
{
    /// <summary>
    ///     Gets or sets the description of the project.
    /// </summary>
    /// <value>The description of the project.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the details of the project.
    /// </summary>
    /// <value>The details.</value>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the URL of the hero image.
    /// </summary>
    /// <value>The URL of the hero image.</value>
    public string HeroUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the ID of the project.
    /// </summary>
    /// <value>The ID of the project.</value>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    ///     Gets or sets the set of languages used for this project.
    /// </summary>
    /// <value>The languages.</value>
    public List<string> Languages { get; set; } = [];

    /// <summary>
    ///     Gets or sets the name of the project.
    /// </summary>
    /// <value>The name of the project.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the rank of the project.
    /// </summary>
    /// <value>The rank of the project.</value>
    public int Rank { get; set; }

    /// <summary>
    ///     Gets or sets the host of the project.
    /// </summary>
    /// <value>The host of the project.</value>
    public string? RemoteTarget { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the project.
    /// </summary>
    /// <value>The URL of the project.</value>
    public string? RemoteUrl { get; set; }

    /// <summary>
    ///     Gets or sets the slug of the project.
    /// </summary>
    /// <value>The slug of the project.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the status of the project.
    /// </summary>
    /// <value>The status of the project.</value>
    public ProjectStatus Status { get; set; } = ProjectStatus.Ongoing;

    /// <summary>
    ///     Gets or sets the tagline of the project.
    /// </summary>
    /// <value>The tagline.</value>
    public string? Tagline { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="Project" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="Project" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="Project" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(Project? left, Project? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="Project" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="Project" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="Project" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(Project? left, Project? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="Project" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(Project? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id.Equals(other.Id);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="Project" /> and equals the
    ///     value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Project other && Equals(other);
    }

    /// <summary>
    ///     Gets the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Id.GetHashCode();
    }
}
