using BoothDotDev.Common.Data.Web;

namespace BoothDotDev.Data.Web;

/// <inheritdoc cref="IBlacklistEntry"/>
internal sealed class BlacklistEntry : IEquatable<BlacklistEntry>, IBlacklistEntry
{
    /// <inheritdoc/>
    public string EmailAddress { get; internal set; } = string.Empty;

    /// <inheritdoc/>
    public string Name { get; internal set; } = string.Empty;

    /// <inheritdoc/>
    public string Reason { get; internal set; } = string.Empty;

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="BlacklistEntry" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="BlacklistEntry" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="BlacklistEntry" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(BlacklistEntry? left, BlacklistEntry? right) => Equals(left, right);

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="BlacklistEntry" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="BlacklistEntry" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="BlacklistEntry" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(BlacklistEntry? left, BlacklistEntry? right) => !(left == right);

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="BlacklistEntry" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(BlacklistEntry? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EmailAddress.Equals(other.EmailAddress);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="BlacklistEntry" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BlacklistEntry other && Equals(other);
    }

    /// <summary>
    ///     Gets the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return EmailAddress.GetHashCode();
    }
}
