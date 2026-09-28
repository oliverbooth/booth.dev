using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class StreakCheckInConfiguration : IEntityTypeConfiguration<StreakCheckIn>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StreakCheckIn> builder)
    {
        builder.ToTable("streak_check_in");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.StreakId).IsRequired();
        builder.Property(e => e.OccurredOn).IsRequired();
        builder.Property(e => e.Kind).IsRequired();
        builder.Property(e => e.Note).IsRequired(false).HasMaxLength(255);

        builder.HasOne<Streak>().WithMany().HasForeignKey(e => e.StreakId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.StreakId, e.OccurredOn }).IsUnique();
    }
}
