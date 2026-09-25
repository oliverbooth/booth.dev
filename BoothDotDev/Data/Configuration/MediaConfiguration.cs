using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.ToTable("media",
            t => t.HasCheckConstraint("ck_media_one_owner", "(project_id IS NULL) <> (creation_id IS NULL)"));
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Position).IsRequired();
        builder.Property(e => e.IsCover).IsRequired();
        builder.Property(e => e.Alt).IsRequired(false).HasMaxLength(1000);
        builder.Property(e => e.Width).IsRequired(false);
        builder.Property(e => e.Height).IsRequired(false);
        builder.Property(e => e.Duration).IsRequired(false);

        builder.HasOne<Project>().WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Creation>().WithMany().HasForeignKey(e => e.CreationId).OnDelete(DeleteBehavior.Cascade);

        // NULLs never collide in a unique index, so each side only constrains its own kind of owner
        builder.HasIndex(e => new { e.ProjectId, e.FileName }).IsUnique();
        builder.HasIndex(e => new { e.CreationId, e.FileName }).IsUnique();

        builder.HasIndex(e => e.ProjectId).IsUnique().HasFilter("is_cover").HasDatabaseName("ux_media_project_cover");
        builder.HasIndex(e => e.CreationId).IsUnique().HasFilter("is_cover").HasDatabaseName("ux_media_creation_cover");
    }
}
