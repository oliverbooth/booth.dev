using BoothDotDev.Data.Models;
using BoothDotDev.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class CreationConfiguration : IEntityTypeConfiguration<Creation>
{
    public void Configure(EntityTypeBuilder<Creation> builder)
    {
        builder.ToTable("creation");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Kind).IsRequired();
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Description).IsRequired(false).HasMaxLength(10000).HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.PublishedAt).IsRequired();
        builder.Property(e => e.Visibility).IsRequired().HasDefaultValue(Visibility.Published).ValueGeneratedNever();
        builder.Property(e => e.IsWorkInProgress).IsRequired();
        builder.Property(e => e.MadeWith).IsRequired(false).HasMaxLength(255);
        builder.Property(e => e.Tools).IsRequired();
        builder.Property(e => e.Tags).IsRequired();
        builder.Property(e => e.Resolution).IsRequired(false).HasConversion<SizeToResolutionConverter>();
        builder.Property(e => e.Duration).IsRequired(false);
        builder.Property(e => e.TrashedAt).IsRequired(false);

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.TrashedAt);
    }
}
