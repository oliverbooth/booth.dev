using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class BlogPostDraftConfiguration : IEntityTypeConfiguration<BlogPostDraft>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BlogPostDraft> builder)
    {
        builder.ToTable("blog_post_draft");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id);
        builder.Property(e => e.BlogPostId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Body).IsRequired();
        builder.Property(e => e.Excerpt).HasMaxLength(512).IsRequired(false);
        builder.Property(e => e.Visibility).IsRequired();
        builder.Property(e => e.Tags).IsRequired();
        builder.Property(e => e.CategoryId).IsRequired();
        builder.Property(e => e.ShowTableOfContents).HasColumnName("show_toc").IsRequired().HasDefaultValue(false);
        builder.Property(e => e.TableOfContentsExpanded).HasColumnName("toc_open").IsRequired().HasDefaultValue(true);

        builder.HasOne<BlogPost>().WithMany().HasForeignKey(e => e.BlogPostId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<BlogPostCategory>().WithMany().HasForeignKey(e => e.CategoryId);

        builder.HasIndex(e => e.BlogPostId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
