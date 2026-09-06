using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="TraktCredential" /> entity.
/// </summary>
internal sealed class TraktCredentialConfiguration : IEntityTypeConfiguration<TraktCredential>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TraktCredential> builder)
    {
        builder.ToTable("trakt_credential");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id).ValueGeneratedNever();
        builder.Property(entry => entry.AccessToken).IsRequired();
        builder.Property(entry => entry.RefreshToken).IsRequired();
        builder.Property(entry => entry.ExpiresAt).IsRequired();
    }
}
