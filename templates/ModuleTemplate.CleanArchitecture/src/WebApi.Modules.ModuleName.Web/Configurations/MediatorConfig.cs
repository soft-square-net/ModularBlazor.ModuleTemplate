using Microsoft.Extensions.DependencyInjection;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Behaviors;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Create;

namespace soft.square.net.WebApi.Modules.ModuleName.Web.Configurations;

public static class MediatorConfig
{
    /// <summary>
    /// The module's mediator. Mediator's source generator writes the handler table into this assembly at compile time
    /// (no reflection, no static caches outside the module), and AddMediator registers it in the module's container,
    /// so it unloads with the module. Each module has its own mediator; modules never send each other messages.
    /// </summary>
    public static IServiceCollection AddMediatorSourceGen(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            // Scoped: handlers use the request's tenant DbContext and the caller's access scopes.
            options.ServiceLifetime = ServiceLifetime.Scoped;

            // One type from each assembly that holds handlers. Not Core: its domain events are dispatched by the
            // platform's TenantDbContext (IDomainEventHandler, in the same save), not by the mediator.
            options.Assemblies =
            [
                typeof(CreateNoteCommand)   // UseCases
            ];

            // Order matters: the first runs outermost.
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>)
            ];
        });
        return services;
    }
}
