using BoothDotDev.Data.Models;
using BoothDotDev.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class ArtworkItemConfiguration : IEntityTypeConfiguration<ArtworkItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArtworkItem> builder)
    {
        builder.ToTable("artwork_item");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Description).IsRequired(false).HasMaxLength(10000).HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.PublishedAt).IsRequired();
        builder.Property(e => e.Visibility).IsRequired().HasDefaultValue(Visibility.Published);
        builder.Property(e => e.IsWorkInProgress).IsRequired();
        builder.Property(e => e.MadeWith).IsRequired(false).HasMaxLength(255);
        builder.Property(e => e.Resolution).IsRequired().HasConversion<SizeToResolutionConverter>();
        builder.Property(e => e.TrashedAt).IsRequired(false);

        builder.HasIndex(e => e.TrashedAt);
    }
}
