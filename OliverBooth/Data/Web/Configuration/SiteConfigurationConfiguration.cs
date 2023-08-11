using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OliverBooth.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="SiteConfiguration" /> entity.
/// </summary>
internal sealed class SiteConfigurationConfiguration : IEntityTypeConfiguration<SiteConfiguration>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SiteConfiguration> builder)
    {
        builder.ToTable("SiteConfig");
        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(1000);
    }
}
