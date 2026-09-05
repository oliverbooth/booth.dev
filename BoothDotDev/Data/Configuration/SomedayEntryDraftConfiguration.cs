using BoothDotDev.Data.Models;
using BoothDotDev.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class SomedayEntryDraftConfiguration : IEntityTypeConfiguration<SomedayEntryDraft>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SomedayEntryDraft> builder)
    {
        builder.ToTable("someday_entry_draft");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.SomedayEntryId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Body).IsRequired().HasMaxLength(10000).HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.Visibility).IsRequired();

        builder.HasOne<SomedayEntry>().WithMany().HasForeignKey(e => e.SomedayEntryId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.SomedayEntryId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
