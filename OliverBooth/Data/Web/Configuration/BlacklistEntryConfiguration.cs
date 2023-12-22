using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OliverBooth.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="BlacklistEntry" /> entity.
/// </summary>
internal sealed class BlacklistEntryConfiguration : IEntityTypeConfiguration<BlacklistEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BlacklistEntry> builder)
    {
        builder.ToTable("ContactBlacklist");
        builder.HasKey(entry => entry.EmailAddress);

        builder.Property(entry => entry.EmailAddress).IsRequired();
        builder.Property(entry => entry.Name).IsRequired();
        builder.Property(entry => entry.Reason).IsRequired();
    }
}
