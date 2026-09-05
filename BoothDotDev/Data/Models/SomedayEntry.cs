using System.ComponentModel.DataAnnotations.Schema;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a single entry on the "someday" page - one "Someday, ..." wish, rendered alongside every other
///     entry on that one page rather than on a permalink of its own.
/// </summary>
public sealed class SomedayEntry : IEquatable<SomedayEntry>, IMarkdownBody
{
    /// <summary>
    ///     Gets the draft that is currently live for this entry.
    /// </summary>
    /// <value>The currently-live draft.</value>
    public SomedayEntryDraft? CurrentDraft { get; internal set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live for this entry.
    /// </summary>
    /// <value>The ID of the currently-live draft.</value>
    public Guid? CurrentDraftId { get; internal set; }

    /// <summary>
    ///     Gets the ID of the entry.
    /// </summary>
    /// <value>The ID of the entry.</value>
    public Guid Id { get; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets the date and time the entry was first published.
    /// </summary>
    /// <value>The publication date and time.</value>
    public DateTimeOffset PublishedAt { get; internal set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the slug of the entry, used as its anchor ID on the someday page.
    /// </summary>
    /// <value>The slug of the entry.</value>
    /// <remarks>
    ///     Set once and left alone afterward - unlike <see cref="Title" />, it never changes when the entry's
    ///     wording is later revised, so a link shared to this entry keeps working no matter how its wish is
    ///     reworded.
    /// </remarks>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the entry's position on the someday page, relative to every other entry.
    /// </summary>
    /// <value>The sort order of the entry. Lower values are rendered first.</value>
    public int SortOrder { get; set; }

    /// <summary>
    ///     Gets the title of the entry, as of its current draft.
    /// </summary>
    /// <value>The title of the entry.</value>
    [NotMapped]
    public string Title
    {
        get => Draft.Title;
    }

    /// <summary>
    ///     Gets or sets the date and time the entry was moved to the trash.
    /// </summary>
    /// <value>The date and time the entry was trashed, or <see langword="null" /> if the entry is not trashed.</value>
    public DateTimeOffset? TrashedAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time the entry was last updated, i.e. the last time <see cref="CurrentDraftId" />
    ///     changed.
    /// </summary>
    /// <value>The update date and time, or <see langword="null" /> if the entry has not been updated.</value>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    ///     Gets the visibility of the entry, as of its current draft.
    /// </summary>
    /// <value>The visibility of the entry.</value>
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
    private SomedayEntryDraft Draft
    {
        get => CurrentDraft ?? throw new InvalidOperationException(
            $"The current draft for someday entry '{Id}' was not loaded. Ensure the query includes '{nameof(CurrentDraft)}'.");
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="SomedayEntry" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(SomedayEntry? other)
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

    /// <inheritdoc />
    [NotMapped]
    public string Body
    {
        get => Draft.Body;
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="SomedayEntry" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="SomedayEntry" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="SomedayEntry" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(SomedayEntry? left, SomedayEntry? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="SomedayEntry" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="SomedayEntry" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="SomedayEntry" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(SomedayEntry? left, SomedayEntry? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="SomedayEntry" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is SomedayEntry other && Equals(other));
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
