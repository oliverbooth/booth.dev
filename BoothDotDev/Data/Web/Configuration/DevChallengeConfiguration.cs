using BoothDotDev.Common.Data;
using BoothDotDev.Data.Web.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        builder.Property(e => e.Id).HasConversion<ShortGuidToStringConverter>().IsRequired();
        builder.Property(e => e.OldId).IsRequired(false);
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Solution).IsRequired(false);
        builder.Property(e => e.ShowSolution).IsRequired();
        builder.Property(e => e.Visibility).HasConversion<EnumToStringConverter<Visibility>>().IsRequired();
        builder.Property(e => e.Password).IsRequired(false);
    }
}
