using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class PortfolioEntryConfiguration : IEntityTypeConfiguration<PortfolioEntry>
{
    public void Configure(EntityTypeBuilder<PortfolioEntry> builder)
    {
        builder.ToTable("portfolio_entry", t => t.HasCheckConstraint(
            "ck_portfolio_entry_one_target", "(project_id IS NULL) <> (creation_id IS NULL)"));
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Position).IsRequired();
        builder.Property(e => e.FeaturedPosition).IsRequired(false);

        // only deleting for good removes an entry. trashing a creation leaves it, so a restored creation returns to its place
        builder.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Creation).WithMany().HasForeignKey(e => e.CreationId).OnDelete(DeleteBehavior.Cascade);

        // NULLs never collide in a unique index, so each side only constrains its own kind
        builder.HasIndex(e => e.ProjectId).IsUnique();
        builder.HasIndex(e => e.CreationId).IsUnique();
    }
}
