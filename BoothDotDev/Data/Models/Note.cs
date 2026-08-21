using System.ComponentModel.DataAnnotations.Schema;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a note.
/// </summary>
public sealed class Note : IEquatable<Note>, IMarkdownBody
{
    /// <inheritdoc />
    [NotMapped]
    string IMarkdownBody.Body
    {
        get => Draft.Content;
    }

    /// <summary>
    ///     Gets the content of the note, as of its current draft.
    /// </summary>
    /// <value>The content of the note.</value>
    [NotMapped]
    public string Content
    {
        get => Draft.Content;
    }

    /// <summary>
    ///     Gets the draft that is currently live for this note.
    /// </summary>
    /// <value>The currently-live draft.</value>
    public NoteDraft? CurrentDraft { get; internal set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live for this note.
    /// </summary>
    /// <value>The ID of the currently-live draft.</value>
    public Guid? CurrentDraftId { get; internal set; }

    /// <summary>
    ///     Gets the font style of the note, as of its current draft.
    /// </summary>
    /// <value>The font style of the note.</value>
    [NotMapped]
    public FontStyle FontStyle
    {
        get => Draft.FontStyle;
    }

    /// <summary>
    ///     Gets the unique identifier for the note.
    /// </summary>
    /// <value>A <see cref="Guid" /> representing the unique identifier for the note.</value>
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets the date and time when the note was published.
    /// </summary>
    /// <value>A <see cref="DateTimeOffset" /> representing the date and time when the note was published.</value>
    public DateTimeOffset Published { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets the title of the note, as of its current draft.
    /// </summary>
    /// <value>The title of the note.</value>
    [NotMapped]
    public string Title
    {
        get => Draft.Title;
    }

    /// <summary>
    ///     Gets or sets the date and time the note was trashed.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTimeOffset" /> representing when the note was trashed, or <see langword="null" /> if it isn't
    ///     trashed.
    /// </value>
    public DateTimeOffset? TrashedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the note was last updated.
    /// </summary>
    /// <value>A <see cref="DateTimeOffset" /> representing the date and time when the note was last updated.</value>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    ///     Gets the visibility of the note, as of its current draft.
    /// </summary>
    /// <value>The visibility of the note.</value>
    [NotMapped]
    public Visibility Visibility
    {
        get => Draft.Visibility;
    }

    /// <summary>
    ///     Gets the currently-live draft, throwing if it has not been loaded.
    /// </summary>
    /// <value>The currently-live draft.</value>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="CurrentDraft" /> was not eager-loaded by the query that produced this instance.
    /// </exception>
    private NoteDraft Draft
    {
        get => CurrentDraft ?? throw new InvalidOperationException(
            $"The current draft for note '{Id}' was not loaded. Ensure the query includes '{nameof(CurrentDraft)}'.");
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="Note" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="Note" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="Note" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(Note? left, Note? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="Note" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="Note" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="Note" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(Note? left, Note? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="Note" /> is equal to another instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(Note? other)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="Note" /> and equals the value of this
    ///     instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Note other && Equals(other);
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
