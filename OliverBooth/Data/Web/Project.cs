namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a project.
/// </summary>
internal sealed class Project : IEquatable<Project>, IProject
{
    /// <inheritdoc />
    public string Description { get; private set; } = string.Empty;

    /// <inheritdoc />
    public string HeroUrl { get; private set; } = string.Empty;

    /// <inheritdoc />
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <inheritdoc />
    public string Name { get; private set; } = string.Empty;

    /// <inheritdoc />
    public int Rank { get; private set; }

    /// <inheritdoc />
    public string? RemoteTarget { get; private set; }

    /// <inheritdoc />
    public string? RemoteUrl { get; private set; }

    /// <inheritdoc />
    public string Slug { get; private set; } = string.Empty;

    /// <inheritdoc />
    public ProjectStatus Status { get; private set; } = ProjectStatus.Ongoing;

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="Project" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="Project" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="Project" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(Project? left, Project? right) => Equals(left, right);

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="Project" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="Project" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="Project" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(Project? left, Project? right) => !(left == right);

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
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
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
