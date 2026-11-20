namespace soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate.Specifications;

/// <summary>One note, tracked (for changing it).</summary>
public sealed class NoteByIdSpec : SingleResultSpecification<Note>
{
    public NoteByIdSpec(int id) => Query.Where(n => n.Id == id);
}

/// <summary>
/// One page of notes, newest first, untracked. <c>CountAsync</c> with the same specification ignores the paging,
/// so it gives the total.
/// </summary>
public sealed class NotesPageSpec : Specification<Note>
{
    public NotesPageSpec(int page, int pageSize) =>
        Query.OrderByDescending(n => n.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .AsNoTracking();
}

/// <summary>
/// One page of the notes whose title or body contains the text, ignoring case (lowered on both sides, so it works the
/// same on every database engine), newest first, untracked.
/// </summary>
public sealed class NoteSearchSpec : Specification<Note>
{
    public NoteSearchSpec(string text, int page, int pageSize)
    {
        var term = text.Trim().ToLowerInvariant();
        Query.Where(n => n.Title.ToLower().Contains(term) || n.Body.ToLower().Contains(term))
            .OrderByDescending(n => n.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .AsNoTracking();
    }
}
