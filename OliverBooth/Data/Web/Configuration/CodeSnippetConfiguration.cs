using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OliverBooth.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Book" /> entity.
/// </summary>
internal sealed class CodeSnippetConfiguration : IEntityTypeConfiguration<CodeSnippet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CodeSnippet> builder)
    {
        builder.ToTable("CodeSnippet");
        builder.HasKey(e => new { e.Id, e.Language });

        builder.Property(e => e.Id);
        builder.Property(e => e.Language);
        builder.Property(e => e.Content);
    }
}
