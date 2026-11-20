using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate.Specifications;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Delete;

/// <summary>Delete a note. <paramref name="Version"/> is the version the caller read, so a changed note is not deleted unseen.</summary>
public sealed record DeleteNoteCommand(int Id, int Version) : ICommand<Result>;

public sealed class DeleteNoteHandler(IRepository<Note> notes) : ICommandHandler<DeleteNoteCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteNoteCommand command, CancellationToken ct)
    {
        var note = await notes.FirstOrDefaultAsync(new NoteByIdSpec(command.Id), ct);
        if (note is null) return Result.NotFound(NoteResults.NotFoundMessage(command.Id));
        if (note.Version != command.Version) return Result.Conflict(NoteResults.StaleMessage(command.Id, note.Version, command.Version));

        await notes.DeleteAsync(note, ct);
        return Result.NoContent();
    }
}
