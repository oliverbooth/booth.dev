namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a programming language.
/// </summary>
public sealed class ProgrammingLanguage : IEquatable<ProgrammingLanguage>
{
    /// <summary>
    ///     Gets the unique key for this programming language.
    /// </summary>
    /// <value>The unique key.</value>
    /// <remarks>This is generally the file extension of the language.</remarks>
    public string Key { get; } = string.Empty;

    /// <summary>
    ///     Gets the name of this programming language.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; internal set; } = string.Empty;

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="ProgrammingLanguage" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="ProgrammingLanguage" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="ProgrammingLanguage" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(ProgrammingLanguage? left, ProgrammingLanguage? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="ProgrammingLanguage" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="ProgrammingLanguage" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="ProgrammingLanguage" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(ProgrammingLanguage? left, ProgrammingLanguage? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="ProgrammingLanguage" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(ProgrammingLanguage? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Key.Equals(other.Key);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="ProgrammingLanguage" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ProgrammingLanguage other && Equals(other);
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
