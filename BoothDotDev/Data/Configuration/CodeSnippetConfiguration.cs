using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Book" /> entity.
/// </summary>
internal sealed class CodeSnippetConfiguration : IEntityTypeConfiguration<CodeSnippet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CodeSnippet> builder)
    {
        builder.ToTable("code_snippet");
        builder.HasKey(e => new { e.Id, e.Language });

        builder.Property(e => e.Id);
        builder.Property(e => e.Language);
        builder.Property(e => e.Content);
    }
}
