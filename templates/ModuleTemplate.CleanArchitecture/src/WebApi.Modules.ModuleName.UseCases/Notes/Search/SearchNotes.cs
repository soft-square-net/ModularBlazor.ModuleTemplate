using Microsoft.Extensions.Options;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate.Specifications;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Search;

/// <summary>One page of the notes whose title or body contains <paramref name="Text"/>, ignoring case.</summary>
public sealed record SearchNotesQuery(string Text, int Page = 1, int PageSize = 20) : IQuery<Result<NotePage>>;

public sealed class SearchNotesHandler(IReadRepository<Note> notes, IOptions<ModuleNameOptions> options)
    : IQueryHandler<SearchNotesQuery, Result<NotePage>>
{
    public const int MinTextLength = 2;

    public async ValueTask<Result<NotePage>> Handle(SearchNotesQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Text) || query.Text.Trim().Length < MinTextLength)
            return Result<NotePage>.Invalid(new ValidationError(nameof(query.Text), $"Search for at least {MinTextLength} characters."));
        var paging = NoteResults.CheckPage(query.Page, query.PageSize, options.Value.MaxPageSize);
        if (!paging.IsSuccess) return paging.As<NotePage>();

        var spec = new NoteSearchSpec(query.Text, query.Page, query.PageSize);
        var items = await notes.ListAsync(spec, ct);
        var total = await notes.CountAsync(spec, ct);
        return new NotePage([.. items.Select(NoteDto.From)], query.Page, query.PageSize, total);
    }
}
