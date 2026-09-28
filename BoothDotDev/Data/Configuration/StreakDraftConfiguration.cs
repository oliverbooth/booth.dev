using BoothDotDev.Data.Models;
using BoothDotDev.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class StreakDraftConfiguration : IEntityTypeConfiguration<StreakDraft>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StreakDraft> builder)
    {
        builder.ToTable("streak_draft");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.StreakId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Body).IsRequired().HasMaxLength(10000).HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.Visibility).IsRequired();
        builder.Property(e => e.Color).IsRequired(false);
        builder.Property(e => e.DotColor).IsRequired(false);
        builder.Property(e => e.CadenceUnit).IsRequired(false);
        builder.Property(e => e.CadenceInterval).IsRequired(false);

        builder.HasOne<Streak>().WithMany().HasForeignKey(e => e.StreakId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.StreakId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
