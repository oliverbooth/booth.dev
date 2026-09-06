using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Watchable" /> entity.
/// </summary>
internal sealed class WatchableConfiguration : IEntityTypeConfiguration<Watchable>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Watchable> builder)
    {
        builder.ToTable("watchable");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id).IsRequired();
        builder.Property(entry => entry.Title).IsRequired().HasMaxLength(128);
        builder.Property(entry => entry.Kind).IsRequired();
        builder.Property(entry => entry.State).IsRequired();
    }
}
