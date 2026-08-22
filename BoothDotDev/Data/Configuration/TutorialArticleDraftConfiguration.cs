using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="TutorialArticleDraft" /> entity.
/// </summary>
internal sealed class TutorialArticleDraftConfiguration : IEntityTypeConfiguration<TutorialArticleDraft>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TutorialArticleDraft> builder)
    {
        builder.ToTable("tutorial_article_draft");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.TutorialArticleId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Body).IsRequired();
        builder.Property(e => e.Excerpt).HasMaxLength(512).IsRequired(false);
        builder.Property(e => e.Folder).IsRequired();
        builder.Property(e => e.Rank).IsRequired();
        builder.Property(e => e.PreviewImageUrl).HasConversion<UriToStringConverter>();
        builder.Property(e => e.ShowTableOfContents).HasColumnName("show_toc").IsRequired().HasDefaultValue(true);
        builder.Property(e => e.TableOfContentsExpanded).HasColumnName("toc_open").IsRequired().HasDefaultValue(true);
        builder.Property(e => e.Visibility).IsRequired();

        builder.HasOne<TutorialArticle>().WithMany().HasForeignKey(e => e.TutorialArticleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<TutorialFolder>().WithMany().HasForeignKey(e => e.Folder);

        builder.HasIndex(e => e.TutorialArticleId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
