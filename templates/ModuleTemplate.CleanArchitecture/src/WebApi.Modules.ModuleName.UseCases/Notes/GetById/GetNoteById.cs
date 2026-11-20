using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.GetById;

public sealed record GetNoteByIdQuery(int Id) : IQuery<Result<NoteDto>>;

public sealed class GetNoteByIdHandler(IReadRepository<Note> notes) : IQueryHandler<GetNoteByIdQuery, Result<NoteDto>>
{
    public async ValueTask<Result<NoteDto>> Handle(GetNoteByIdQuery query, CancellationToken ct) =>
        await notes.GetByIdAsync(query.Id, ct) is { } note
            ? NoteDto.From(note)
            : Result<NoteDto>.NotFound(NoteResults.NotFoundMessage(query.Id));
}
