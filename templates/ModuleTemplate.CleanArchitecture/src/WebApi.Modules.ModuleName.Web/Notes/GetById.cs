using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.GetById;

namespace soft.square.net.WebApi.Modules.ModuleName.Web.Notes;

/// <summary>GET /notes/{id}: one note.</summary>
public sealed class GetById : IEndpoint
{
    public static RouteHandlerBuilder Map(IEndpointRouteBuilder notes) =>
        notes.MapGet("/{id:int}", HandleAsync)
            .RequirePermission(ModuleNamePermissions.Read)
            .WithName("ModuleName_GetNote");

    private static async Task<Results<Ok<NoteDto>, ProblemHttpResult>> HandleAsync(int id, ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetNoteByIdQuery(id), ct)).ToOkResult();
}
