using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Blog.Configuration;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.EmailAddress).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Password).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Salt).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Registered).IsRequired();
    }
}
