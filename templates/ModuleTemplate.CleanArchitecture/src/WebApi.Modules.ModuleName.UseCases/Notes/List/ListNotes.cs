using Microsoft.Extensions.Options;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate.Specifications;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.List;

/// <summary>One page of the tenant's notes, newest first.</summary>
public sealed record ListNotesQuery(int Page = 1, int PageSize = 20) : IQuery<Result<NotePage>>;

public sealed class ListNotesHandler(IReadRepository<Note> notes, IOptions<ModuleNameOptions> options)
    : IQueryHandler<ListNotesQuery, Result<NotePage>>
{
    public async ValueTask<Result<NotePage>> Handle(ListNotesQuery query, CancellationToken ct)
    {
        var paging = NoteResults.CheckPage(query.Page, query.PageSize, options.Value.MaxPageSize);
        if (!paging.IsSuccess) return paging.As<NotePage>();

        var spec = new NotesPageSpec(query.Page, query.PageSize);
        var items = await notes.ListAsync(spec, ct);
        var total = await notes.CountAsync(spec, ct); // counts without the paging
        return new NotePage([.. items.Select(NoteDto.From)], query.Page, query.PageSize, total);
    }
}
