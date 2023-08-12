using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OliverBooth.Blog.Data.Configuration;

internal sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPost");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id);
        builder.Property(e => e.WordPressId).IsRequired(false);
        builder.Property(e => e.Slug).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AuthorId).IsRequired();
        builder.Property(e => e.Published).IsRequired();
        builder.Property(e => e.Updated).IsRequired(false);
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Body).IsRequired();
        builder.Property(e => e.IsRedirect).IsRequired();
        builder.Property(e => e.RedirectUrl).HasConversion<UriToStringConverter>().HasMaxLength(255).IsRequired(false);
        builder.Property(e => e.EnableComments).IsRequired();
        builder.Property(e => e.DisqusDomain).IsRequired(false);
        builder.Property(e => e.DisqusIdentifier).IsRequired(false);
        builder.Property(e => e.DisqusPath).IsRequired(false);
    }
}
