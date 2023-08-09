namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a site configuration item.
/// </summary>
public sealed class SiteConfiguration : IEquatable<SiteConfiguration>
{
    /// <summary>
    ///     Gets or sets the name of the configuration item.
    /// </summary>
    /// <value>The name of the configuration item.</value>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the value of the configuration item.
    /// </summary>
    /// <value>The value of the configuration item.</value>
    public string? Value { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="SiteConfiguration" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="SiteConfiguration" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="SiteConfiguration" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(SiteConfiguration? left, SiteConfiguration? right) => Equals(left, right);

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="SiteConfiguration" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="SiteConfiguration" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="SiteConfiguration" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(SiteConfiguration? left, SiteConfiguration? right) => !(left == right);

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="SiteConfiguration" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(SiteConfiguration? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key;
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="SiteConfiguration" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is SiteConfiguration other && Equals(other);
    }

    /// <summary>
    ///     Gets the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Key.GetHashCode();
    }
}
