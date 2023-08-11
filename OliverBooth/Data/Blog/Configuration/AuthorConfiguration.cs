using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OliverBooth.Data.Blog.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Author" /> entity.
/// </summary>
internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Author");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.EmailAddress).HasMaxLength(255).IsRequired(false);
    }
}
