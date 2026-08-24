using BoothDotDev.Data.Models;
using BoothDotDev.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="DevChallengeDraft" /> entity.
/// </summary>
internal sealed class DevChallengeDraftConfiguration : IEntityTypeConfiguration<DevChallengeDraft>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DevChallengeDraft> builder)
    {
        builder.ToTable("dev_challenge_draft");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.DevChallengeId).HasConversion<ShortGuidToGuidConverter>().IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Description).IsRequired().HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.Excerpt).HasMaxLength(512).IsRequired(false).HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.Solution).IsRequired(false).HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.ShowSolution).IsRequired();
        builder.Property(e => e.Visibility).IsRequired();

        builder.HasOne<DevChallenge>().WithMany().HasForeignKey(e => e.DevChallengeId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.DevChallengeId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
