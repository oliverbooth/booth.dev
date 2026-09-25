using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable("link", t => t.HasCheckConstraint("ck_link_one_owner", "(project_id IS NULL) <> (creation_id IS NULL)"));
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Kind).IsRequired();
        builder.Property(e => e.Url).IsRequired().HasMaxLength(2048);
        builder.Property(e => e.Label).IsRequired(false).HasMaxLength(100);
        builder.Property(e => e.Position).IsRequired();
        builder.Property(e => e.IsPrimary).IsRequired();

        builder.HasOne<Project>().WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Creation>().WithMany().HasForeignKey(e => e.CreationId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ProjectId).IsUnique().HasFilter("is_primary").HasDatabaseName("ux_link_project_primary");
        builder.HasIndex(e => e.CreationId).IsUnique().HasFilter("is_primary").HasDatabaseName("ux_link_creation_primary");
    }
}
