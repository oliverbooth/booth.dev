using BoothDotDev.Data.Models;
using BoothDotDev.Data.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class NoteDraftConfiguration : IEntityTypeConfiguration<NoteDraft>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<NoteDraft> builder)
    {
        builder.ToTable("note_draft");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.NoteId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Content).IsRequired().HasMaxLength(10000).HasConversion<MarkdownValueConverter>();
        builder.Property(e => e.FontStyle).IsRequired().HasDefaultValue(FontStyle.Serif);
        builder.Property(e => e.Visibility).IsRequired();

        builder.HasOne<Note>().WithMany().HasForeignKey(e => e.NoteId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.NoteId);
        builder.HasIndex(e => e.CreatedAt);
    }
}
