using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Blog.Configuration;

internal sealed class LegacyCommentConfiguration : IEntityTypeConfiguration<LegacyComment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LegacyComment> builder)
    {
        builder.ToTable("legacy_comment");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.PostId).IsRequired();
        builder.Property(e => e.Author).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Avatar).IsRequired(false).HasMaxLength(32767);
        builder.Property(e => e.Body).IsRequired().HasMaxLength(32767);
        builder.Property(e => e.ParentComment).IsRequired(false);
    }
}
