using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Playable" /> entity.
/// </summary>
internal sealed class PlayableConfiguration : IEntityTypeConfiguration<Playable>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Playable> builder)
    {
        builder.ToTable("playable");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id).IsRequired();
        builder.Property(entry => entry.Title).IsRequired().HasMaxLength(128);
        builder.Property(entry => entry.State).IsRequired();
        builder.Property(entry => entry.IgdbSlug).IsRequired(false).HasMaxLength(128);

        builder.HasIndex(entry => entry.IgdbSlug).IsUnique().HasFilter("igdb_slug IS NOT NULL");
    }
}
