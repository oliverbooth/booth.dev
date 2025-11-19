using BoothDotDev.Data.Web.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Web.Configuration;

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
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Solution).IsRequired(false);
        builder.Property(e => e.ShowSolution).IsRequired();
        builder.Property(e => e.Visibility).HasColumnType("visibility").IsRequired();
        builder.Property(e => e.Password).IsRequired(false);
    }
}
