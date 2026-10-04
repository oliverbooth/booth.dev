using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="PlayableEdition" /> entity.
/// </summary>
internal sealed class PlayableEditionConfiguration : IEntityTypeConfiguration<PlayableEdition>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PlayableEdition> builder)
    {
        builder.ToTable("playable_edition");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id).IsRequired();
        builder.Property(entry => entry.PlayableId).IsRequired();
        builder.Property(entry => entry.Label).IsRequired().HasMaxLength(64);
        builder.Property(entry => entry.IgdbSlug).IsRequired(false).HasMaxLength(128);
        builder.Property(entry => entry.Position).IsRequired();
        builder.PrimitiveCollection(entry => entry.Platforms).ElementType().HasConversion<string>();

        builder.HasIndex(entry => entry.PlayableId);
    }
}
