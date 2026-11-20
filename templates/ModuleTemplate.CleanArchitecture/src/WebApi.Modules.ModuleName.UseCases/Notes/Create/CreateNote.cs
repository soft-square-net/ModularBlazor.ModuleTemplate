using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Create;

/// <summary>Write a new note; the caller becomes its author.</summary>
public sealed record CreateNoteCommand(string Title, string? Body) : ICommand<Result<NoteDto>>;

public sealed class CreateNoteHandler(IRepository<Note> notes, IAccessScopes access, TimeProvider clock)
    : ICommandHandler<CreateNoteCommand, Result<NoteDto>>
{
    public async ValueTask<Result<NoteDto>> Handle(CreateNoteCommand command, CancellationToken ct)
    {
        if (access.UserId is not { Length: > 0 } me) return Result<NoteDto>.Unauthorized("Sign in first.");

        var note = await notes.AddAsync(Note.Create(me, command.Title, command.Body, clock.GetUtcNow()), ct);
        return NoteDto.From(note);
    }
}
