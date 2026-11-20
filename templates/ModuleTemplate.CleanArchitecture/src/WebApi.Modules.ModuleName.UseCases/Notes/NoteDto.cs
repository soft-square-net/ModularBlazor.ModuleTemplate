using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes;

/// <summary>A note as the API returns it. <see cref="Version"/> goes back with an edit or a delete.</summary>
public sealed record NoteDto(int Id, string Title, string Body, string CreatedBy, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, int Version)
{
    public static NoteDto From(Note note) =>
        new(note.Id, note.Title, note.Body, note.CreatedBy, note.CreatedAt, note.UpdatedAt, note.Version);
}

/// <summary>One page of notes and the total across all pages.</summary>
public sealed record NotePage(IReadOnlyList<NoteDto> Items, int Page, int PageSize, int Total);

/// <summary>Small helpers the note use cases share.</summary>
public static class NoteResults
{
    /// <summary>Checks a requested page against the module's settings.</summary>
    public static Result CheckPage(int page, int pageSize, int maxPageSize) =>
        page < 1 ? Result.Invalid(new ValidationError(nameof(page), "The page starts at 1."))
        : pageSize < 1 || pageSize > maxPageSize ? Result.Invalid(new ValidationError(nameof(pageSize), $"The page size is 1 to {maxPageSize}."))
        : Result.Success();

    public static string NotFoundMessage(int id) => $"Note {id} was not found.";

    public static string StaleMessage(int id, int current, int read) =>
        $"Note {id} is at version {current}, not {read}; reload it and try again.";
}
