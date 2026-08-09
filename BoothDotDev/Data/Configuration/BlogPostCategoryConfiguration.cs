using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class BlogPostCategoryConfiguration : IEntityTypeConfiguration<BlogPostCategory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BlogPostCategory> builder)
    {
        builder.ToTable("blog_post_category");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id).IsRequired();
        builder.Property(entry => entry.Name).IsRequired();
        builder.Property(entry => entry.Slug).IsRequired();
        builder.Property(entry => entry.FontStyle).IsRequired();
        builder.Property(entry => entry.ParentCategoryId).IsRequired(false);

        builder.HasOne(entry => entry.ParentCategory)
            .WithMany(entry => entry.Children)
            .HasForeignKey(entry => entry.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entry => entry.Slug).IsUnique();
    }
}
