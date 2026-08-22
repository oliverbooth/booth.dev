using BoothDotDev.Data.Models;
using BoothDotDev.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="DevChallenge" /> entity.
/// </summary>
internal sealed class DevChallengeConfiguration : IEntityTypeConfiguration<DevChallenge>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DevChallenge> builder)
    {
        builder.ToTable("dev_challenge");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasConversion<ShortGuidToGuidConverter>().IsRequired();
        builder.Property(e => e.OldId).IsRequired(false);
        builder.Property(e => e.Password).IsRequired(false);
        builder.Property(e => e.PublishedAt).IsRequired();
        builder.Property(e => e.Updated).IsRequired(false);
        builder.Property(e => e.CurrentDraftId).IsRequired(false);
        builder.Property(e => e.TrashedAt).IsRequired(false);

        builder.HasOne(e => e.CurrentDraft).WithMany().HasForeignKey(e => e.CurrentDraftId);

        builder.HasIndex(e => e.TrashedAt);
    }
}
