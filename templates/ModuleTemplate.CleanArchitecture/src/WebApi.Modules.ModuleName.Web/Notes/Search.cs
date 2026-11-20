using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Search;

namespace soft.square.net.WebApi.Modules.ModuleName.Web.Notes;

/// <summary>GET /notes/search?text=&amp;page=&amp;pageSize=: notes whose title or body contains the text, ignoring case.</summary>
public sealed class Search : IEndpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder notes) =>
        notes.MapGet("/search", HandleAsync)
            .RequirePermission(ModuleNamePermissions.Read)
            .WithName("ModuleName_SearchNotes")
            .WithSummary("Notes whose title or body contains the text (at least 2 characters), ignoring case.");

    private static async Task<Results<Ok<NotePage>, ProblemHttpResult>> HandleAsync(string text, ISender sender, CancellationToken ct, int page = 1, int pageSize = 20) =>
        (await sender.Send(new SearchNotesQuery(text, page, pageSize), ct)).ToOkResult();
}
