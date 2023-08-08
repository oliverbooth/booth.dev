using SmartFormat;
using SmartFormat.Core.Extensions;

namespace OliverBooth.Data.Web;

/// <summary>
///     Represents a MediaWiki-style template.
/// </summary>
public sealed class ArticleTemplate : IEquatable<ArticleTemplate>
{
    /// <summary>
    ///     Gets or sets the format string.
    /// </summary>
    /// <value>The format string.</value>
    public string FormatString { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the name of the template.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="ArticleTemplate" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="ArticleTemplate" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="ArticleTemplate" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(ArticleTemplate? left, ArticleTemplate? right) => Equals(left, right);

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="ArticleTemplate" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="ArticleTemplate" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="ArticleTemplate" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(ArticleTemplate? left, ArticleTemplate? right) => !(left == right);

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="ArticleTemplate" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(ArticleTemplate? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="ArticleTemplate" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ArticleTemplate other && Equals(other);
    }

    /// <summary>
    ///     Gets the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Name.GetHashCode();
    }
}
