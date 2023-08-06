namespace OliverBooth.Data.Blog;

/// <summary>
///     Represents an author of a blog post.
/// </summary>
public sealed class Author : IEquatable<Author>
{
    /// <summary>
    ///     Gets or sets the email address of the author.
    /// </summary>
    /// <value>The email address.</value>
    public string? EmailAddress { get; set; }

    /// <summary>
    ///     Gets the ID of the author.
    /// </summary>
    /// <value>The ID.</value>
    public int Id { get; private set; }

    /// <summary>
    ///     Gets or sets the name of the author.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; } = string.Empty;

    public bool Equals(Author? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Author other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}
