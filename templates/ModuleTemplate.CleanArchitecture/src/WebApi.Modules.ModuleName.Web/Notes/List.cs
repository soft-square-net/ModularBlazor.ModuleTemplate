using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.List;

namespace soft.square.net.WebApi.Modules.ModuleName.Web.Notes;

/// <summary>GET /notes?page=&amp;pageSize=: one page of the tenant's notes, newest first.</summary>
public sealed class List : IEndpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder notes) =>
        notes.MapGet("/", HandleAsync)
            .RequirePermission(ModuleNamePermissions.Read)
            .WithName("ModuleName_ListNotes")
            .WithSummary("One page of notes, newest first.");

    private static async Task<Results<Ok<NotePage>, ProblemHttpResult>> HandleAsync(ISender sender, CancellationToken ct, int page = 1, int pageSize = 20) =>
        (await sender.Send(new ListNotesQuery(page, pageSize), ct)).ToOkResult();
}
