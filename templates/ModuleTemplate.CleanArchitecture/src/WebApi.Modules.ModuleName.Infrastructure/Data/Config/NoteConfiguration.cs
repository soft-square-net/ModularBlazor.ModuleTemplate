using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;

namespace soft.square.net.WebApi.Modules.ModuleName.Infrastructure.Data.Config;

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> note)
    {
        note.ToTable("notes");
        note.Property(n => n.Title).HasMaxLength(Note.TitleMaxLength);
        // Bounded, so every engine stores it as an ordinary string column (Oracle: NVARCHAR2(2000)).
        note.Property(n => n.Body).HasMaxLength(Note.BodyMaxLength);
        note.Property(n => n.CreatedBy).HasMaxLength(DataSchemaConstants.UserIdLength);
        note.Property(n => n.Version).IsConcurrencyToken();
    }
}

/// <summary>Column sizes shared by the mappings.</summary>
public static class DataSchemaConstants
{
    /// <summary>A user id (the token's <c>sub</c>).</summary>
    public const int UserIdLength = 128;
}
