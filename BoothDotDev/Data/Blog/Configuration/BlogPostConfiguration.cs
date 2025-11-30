using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.Blog.Configuration;

internal sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("blog_post");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id);
        builder.Property(e => e.WordPressId).HasColumnName("wordpress_id").IsRequired(false);
        builder.Property(e => e.Slug).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AuthorId).IsRequired();
        builder.Property(e => e.Published).IsRequired();
        builder.Property(e => e.Updated).IsRequired(false);
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Body).IsRequired();
        builder.Property(e => e.Excerpt).HasMaxLength(512).IsRequired(false);
        builder.Property(e => e.IsRedirect).IsRequired();
        builder.Property(e => e.RedirectUrl).HasConversion<UriToStringConverter>().HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.EnableComments).IsRequired();
        builder.Property(e => e.DisqusDomain).IsRequired(false);
        builder.Property(e => e.DisqusIdentifier).IsRequired(false);
        builder.Property(e => e.DisqusPath).IsRequired(false);
        builder.Property(e => e.Visibility).HasColumnType("visibility").IsRequired();
        builder.Property(e => e.Password).HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.Tags).IsRequired();
        builder.Property(e => e.ShowTableOfContents).HasColumnName("show_toc").IsRequired().HasDefaultValue(false);
        builder.Property(e => e.TableOfContentsExpanded).HasColumnName("toc_open").IsRequired().HasDefaultValue(true);
    }
}
