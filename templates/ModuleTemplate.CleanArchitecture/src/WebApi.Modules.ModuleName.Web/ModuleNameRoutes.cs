using Carter;
using Notes = soft.square.net.WebApi.Modules.ModuleName.Web.Notes;

namespace soft.square.net.WebApi.Modules.ModuleName.Web;

/// <summary>
/// The module's route table, and its only Carter module (the runtime maps the Carter modules of the entry assembly,
/// under <c>/api/modulename</c>). Every endpoint is listed here once, in the order of the OpenAPI document. A new
/// endpoint: add its file under the resource folder, then one line here.
/// </summary>
public sealed class ModuleNameRoutes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Problem details for domain rules and lost races, on the module's own group (module code, unloads with it).
        var notes = app.MapGroup("/notes").WithTags("Notes").AddEndpointFilter<ModuleNameProblemsFilter>();

        Notes.List.Map(notes);
        Notes.Search.Map(notes);
        Notes.GetById.Map(notes);
        Notes.Create.Map(notes);
        Notes.Edit.Map(notes);
        Notes.Delete.Map(notes);
    }
}
