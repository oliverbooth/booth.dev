using System.ComponentModel.DataAnnotations.Schema;

namespace BoothDotDev.Data.Models;

/// <summary>
///     Represents a devlog entry for a project.
/// </summary>
public sealed class ProjectDevlog : IEquatable<ProjectDevlog>, IMarkdownBody
{
    /// <inheritdoc />
    [NotMapped]
    string IMarkdownBody.Body
    {
        get => Draft.Body;
    }

    /// <summary>
    ///     Gets the body of this devlog entry, as of its current draft.
    /// </summary>
    /// <value>The body.</value>
    [NotMapped]
    public string Body
    {
        get => Draft.Body;
    }

    /// <summary>
    ///     Gets the draft that is currently live for this devlog entry.
    /// </summary>
    /// <value>The currently-live draft.</value>
    public ProjectDevlogDraft? CurrentDraft { get; internal set; }

    /// <summary>
    ///     Gets the ID of the draft that is currently live for this devlog entry.
    /// </summary>
    /// <value>The ID of the currently-live draft.</value>
    public Guid? CurrentDraftId { get; internal set; }

    /// <summary>
    ///     Gets or sets a value indicating whether comments are enabled for the devlog entry.
    /// </summary>
    /// <value><see langword="true" /> if comments are enabled; otherwise, <see langword="false" />.</value>
    public bool EnableComments { get; set; } = true;

    /// <summary>
    ///     Gets the unique identifier for the devlog entry.
    /// </summary>
    /// <value>The unique identifier for the devlog entry.</value>
    public Guid Id { get; internal set; } = Guid.CreateVersion7();

    /// <summary>
    ///     Gets the unique identifier for the project to which this devlog entry belongs.
    /// </summary>
    /// <value>The unique identifier for the project.</value>
    public Guid ProjectId { get; internal set; }

    /// <summary>
    ///     Gets or sets the publication date and time of the devlog entry.
    /// </summary>
    /// <value>The publication date and time of the devlog entry.</value>
    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the slug (URL-friendly identifier) for the devlog entry.
    /// </summary>
    /// <value>The slug (URL-friendly identifier) for the devlog entry.</value>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the title of this devlog entry, as of its current draft.
    /// </summary>
    /// <value>The title.</value>
    [NotMapped]
    public string Title
    {
        get => Draft.Title;
    }

    /// <summary>
    ///     Gets or sets the date and time the devlog entry was trashed.
    /// </summary>
    /// <value>
    ///     The date and time the devlog entry was trashed, or <see langword="null" /> if it isn't trashed.
    /// </value>
    public DateTimeOffset? TrashedAt { get; set; }

    /// <summary>
    ///     Gets or sets the last updated date and time of the devlog entry.
    /// </summary>
    /// <value>The last updated date and time of the devlog entry.</value>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    ///     Gets the visibility of this devlog entry, as of its current draft.
    /// </summary>
    /// <value>The visibility.</value>
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
    private ProjectDevlogDraft Draft
    {
        get => CurrentDraft ?? throw new InvalidOperationException(
            $"The current draft for devlog entry '{Id}' was not loaded. Ensure the query includes '{nameof(CurrentDraft)}'.");
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="ProjectDevlog" /> are equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="ProjectDevlog" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="ProjectDevlog" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(ProjectDevlog? left, ProjectDevlog? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Returns a value indicating whether two instances of <see cref="ProjectDevlog" /> are not equal.
    /// </summary>
    /// <param name="left">The first instance of <see cref="ProjectDevlog" /> to compare.</param>
    /// <param name="right">The second instance of <see cref="ProjectDevlog" /> to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(ProjectDevlog? left, ProjectDevlog? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Returns a value indicating whether this instance of <see cref="ProjectDevlog" /> is equal to another
    ///     instance.
    /// </summary>
    /// <param name="other">An instance to compare with this instance.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(ProjectDevlog? other)
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
    ///     <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="ProjectDevlog" /> and
    ///     equals the value of this instance; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ProjectDevlog other && Equals(other);
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
