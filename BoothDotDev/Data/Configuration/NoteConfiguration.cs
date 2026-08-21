using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("note");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Published).IsRequired();
        builder.Property(e => e.Updated).IsRequired(false);
        builder.Property(e => e.CurrentDraftId).IsRequired(false);
        builder.Property(e => e.TrashedAt).IsRequired(false);

        builder.HasOne(e => e.CurrentDraft).WithMany().HasForeignKey(e => e.CurrentDraftId);

        builder.HasIndex(e => e.TrashedAt);
    }
}
