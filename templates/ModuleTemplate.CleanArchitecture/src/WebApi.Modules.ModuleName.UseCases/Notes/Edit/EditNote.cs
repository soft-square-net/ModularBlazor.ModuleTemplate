using soft.square.net.WebApi.Contracts.Messaging;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate.Specifications;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Edit;

/// <summary>Change a note's title and body. <paramref name="Version"/> is the version the caller read.</summary>
public sealed record EditNoteCommand(int Id, int Version, string Title, string? Body) : ICommand<Result<NoteDto>>;

/// <summary>
/// Load, check the caller's version, change through the aggregate, save. A change that races this one between the
/// check and the save fails on the version's concurrency token (409 from the problems filter).
/// <para>It also publishes <see cref="ModuleNameEvents.NoteEdited"/>: <c>Publish</c> only adds a row to the module's
/// outbox, saved by the same <c>UpdateAsync</c> as the change, and sent once that save commits (never when it fails).</para>
/// </summary>
public sealed class EditNoteHandler(IRepository<Note> notes, IIntegrationEvents events, TimeProvider clock) : ICommandHandler<EditNoteCommand, Result<NoteDto>>
{
    public async ValueTask<Result<NoteDto>> Handle(EditNoteCommand command, CancellationToken ct)
    {
        var note = await notes.FirstOrDefaultAsync(new NoteByIdSpec(command.Id), ct);
        if (note is null) return Result<NoteDto>.NotFound(NoteResults.NotFoundMessage(command.Id));
        if (note.Version != command.Version) return Result<NoteDto>.Conflict(NoteResults.StaleMessage(command.Id, note.Version, command.Version));

        note.Edit(command.Title, command.Body, clock.GetUtcNow());
        events.Publish(ModuleNameEvents.NoteEdited, new NoteEdited(note.Id, note.Version, note.Title), ModuleNameEvents.NoteSubject(note.Id));
        await notes.UpdateAsync(note, ct);
        return NoteDto.From(note);
    }
}
