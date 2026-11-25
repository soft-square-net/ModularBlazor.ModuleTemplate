using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using soft.square.net.WebApi.Contracts.Messaging;
using soft.square.net.WebApi.Modules.ModuleName.Infrastructure;
using soft.square.net.WebApi.Modules.ModuleName.UseCases;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Events;

namespace soft.square.net.WebApi.Modules.ModuleName.Web.Configurations;

public static class ServiceConfigs
{
    /// <summary>
    /// Everything the module registers, in the module's own container (never the host's): settings, data access, any
    /// services the use cases share, and the mediator with every handler.
    /// </summary>
    public static IServiceCollection AddServiceConfigs(this IServiceCollection services, IConfiguration settings)
    {
        services.Configure<ModuleNameOptions>(settings.GetSection("ModuleName"));
        services.AddInfrastructureServices();

        // Services shared by several use cases go here (AddScoped<...>()); Mediator handlers need no registration.

        // Integration event handlers, by event type pattern; the same patterns are the manifest's Subscriptions.
        services.AddIntegrationHandler<NoteEditedLiveHandler>(ModuleNameEvents.AnyNoteEvent);

        services.AddMediatorSourceGen();
        return services;
    }
}
