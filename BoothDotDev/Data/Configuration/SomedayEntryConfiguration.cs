using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class SomedayEntryConfiguration : IEntityTypeConfiguration<SomedayEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SomedayEntry> builder)
    {
        builder.ToTable("someday_entry");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(255);
        builder.Property(e => e.SortOrder).IsRequired();
        builder.Property(e => e.PublishedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired(false);
        builder.Property(e => e.CurrentDraftId).IsRequired(false);
        builder.Property(e => e.TrashedAt).IsRequired(false);

        builder.HasOne(e => e.CurrentDraft).WithMany().HasForeignKey(e => e.CurrentDraftId);

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.SortOrder);
        builder.HasIndex(e => e.TrashedAt);
    }
}
