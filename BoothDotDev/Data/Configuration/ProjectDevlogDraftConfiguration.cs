using BoothDotDev.Data.Models;
using BoothDotDev.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="ProjectDevlogDraft" /> entity.
/// </summary>
internal sealed class ProjectDevlogDraftConfiguration : IEntityTypeConfiguration<ProjectDevlogDraft>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectDevlogDraft> builder)
    {
        builder.ToTable("devlog_draft");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProjectDevlogId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Body).IsRequired().HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.Visibility).IsRequired();

        builder.HasOne<ProjectDevlog>().WithMany().HasForeignKey(e => e.ProjectDevlogId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ProjectDevlogId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
