using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OliverBooth.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Book" /> entity.
/// </summary>
internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Book");
        builder.HasKey(entry => entry.Isbn);

        builder.Property(entry => entry.Isbn).IsRequired();
        builder.Property(entry => entry.Title).IsRequired();
        builder.Property(entry => entry.Author).IsRequired();
        builder.Property(entry => entry.State).HasConversion<EnumToStringConverter<BookState>>().IsRequired();
    }
}
