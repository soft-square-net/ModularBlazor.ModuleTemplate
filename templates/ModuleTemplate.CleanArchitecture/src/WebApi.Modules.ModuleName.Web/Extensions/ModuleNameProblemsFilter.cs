using Microsoft.EntityFrameworkCore;
using soft.square.net.WebApi.SharedKernel;

namespace soft.square.net.WebApi.Modules.ModuleName.Web.Extensions;

/// <summary>
/// The exceptions that still reach the endpoints, as problem details: a broken domain rule (an aggregate throws
/// <see cref="DomainException"/>) and a lost race on a concurrency token are 409. Expected outcomes come back as an
/// Ardalis Result instead (see <see cref="ResultExtensions"/>). On the module's route group, so it is module code and
/// unloads with the module.
/// </summary>
public sealed class ModuleNameProblemsFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try { return await next(context); }
        catch (DbUpdateConcurrencyException) { return Results.Problem("Someone else changed this first; reload it and try again.", statusCode: StatusCodes.Status409Conflict); }
        catch (DomainException e) { return Results.Problem(e.Message, statusCode: StatusCodes.Status409Conflict); }
    }
}
