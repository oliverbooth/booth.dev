using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="PasskeyCredential" /> entity.
/// </summary>
internal sealed class PasskeyCredentialConfiguration : IEntityTypeConfiguration<PasskeyCredential>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PasskeyCredential> builder)
    {
        builder.ToTable("passkey_credential");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.CredentialId).IsRequired();
        builder.Property(e => e.PublicKey).IsRequired();
        builder.Property(e => e.SignatureCounter).IsRequired();
        builder.Property(e => e.AaGuid).IsRequired();
        builder.Property(e => e.Transports).IsRequired(false);
        builder.Property(e => e.Nickname).HasMaxLength(50).IsRequired(false);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.LastUsedAt).IsRequired(false);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.CredentialId).IsUnique();
    }
}
