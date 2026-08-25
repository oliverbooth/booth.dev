namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single immutable snapshot of a note's content, taken at the moment it was saved.
/// </summary>
public sealed class NoteDraft : IEquatable<NoteDraft>, IMarkdownBody
{
    /// <inheritdoc />
    string IMarkdownBody.Body
    {
        get => Content;
    }

    /// <summary>
    ///     Gets or sets the content of the note, as of this draft.
    /// </summary>
    /// <value>The content of the note.</value>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the date and time this draft was saved.
    /// </summary>
    /// <value>The date and time this draft was saved.</value>
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the font style of the note, as of this draft.
    /// </summary>
    /// <value>The font style of the note.</value>
    public FontStyle FontStyle { get; set; } = FontStyle.Serif;

    /// <summary>
    ///     Gets the ID of this draft.
    /// </summary>
    /// <value>The ID of this draft.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets the ID of the note this draft belongs to.
    /// </summary>
    /// <value>The ID of the parent note.</value>
    public Guid NoteId { get; internal set; }

    /// <summary>
    ///     Gets or sets the title of the note, as of this draft.
    /// </summary>
    /// <value>The title of the note.</value>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the visibility of the note, as of this draft.
    /// </summary>
    /// <value>The visibility of the note.</value>
    public Visibility Visibility { get; set; }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="NoteDraft" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="NoteDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="NoteDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(NoteDraft? left, NoteDraft? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="NoteDraft" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="NoteDraft" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="NoteDraft" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(NoteDraft? left, NoteDraft? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="NoteDraft" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(NoteDraft? other)
    {
        if (other is null)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="NoteDraft" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is NoteDraft other && Equals(other);
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
