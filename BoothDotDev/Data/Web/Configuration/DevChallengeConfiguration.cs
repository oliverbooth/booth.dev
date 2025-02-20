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
        builder.ToTable("DevChallenge");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Solution).IsRequired(false);
        builder.Property(e => e.ShowSolution).IsRequired();
        builder.Property(e => e.Published).IsRequired();
    }
}
