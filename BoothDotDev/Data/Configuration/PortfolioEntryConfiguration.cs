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

        // deleting a project or creation for good takes its entry with it. trashing a creation leaves the entry alone, so
        // a restored creation comes back in the place it left
        builder.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Creation).WithMany().HasForeignKey(e => e.CreationId).OnDelete(DeleteBehavior.Cascade);

        // an item is listed at most once. NULLs never collide in a unique index, so each side only limits its own kind
        builder.HasIndex(e => e.ProjectId).IsUnique();
        builder.HasIndex(e => e.CreationId).IsUnique();
    }
}
