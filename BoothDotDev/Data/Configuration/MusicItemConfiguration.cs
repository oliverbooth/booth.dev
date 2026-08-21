using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class MusicItemConfiguration : IEntityTypeConfiguration<MusicItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MusicItem> builder)
    {
        builder.ToTable("music_item");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Description).IsRequired(false).HasMaxLength(10000);
        builder.Property(e => e.Published).IsRequired();
        builder.Property(e => e.Visibility).IsRequired().HasDefaultValue(Visibility.Published);
        builder.Property(e => e.IsWorkInProgress).IsRequired();
        builder.Property(e => e.MadeWith).IsRequired(false).HasMaxLength(255);
        builder.Property(e => e.Duration).IsRequired();
    }
}
