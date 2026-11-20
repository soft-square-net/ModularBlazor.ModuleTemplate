using Microsoft.Extensions.DependencyInjection;
using soft.square.net.WebApi.Data;
using soft.square.net.WebApi.Modules.ModuleName.Infrastructure.Data;

namespace soft.square.net.WebApi.Modules.ModuleName.Infrastructure;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// The module's data access, in the module's own container: the tenant DbContext, and through it
    /// <c>IRepository&lt;T&gt;</c> / <c>IReadRepository&lt;T&gt;</c> for every aggregate root (Note) and the
    /// domain event dispatcher. The connection comes from the tenant, so there is no connection string here.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddTenantDbContext<ModuleNameDbContext>();
        return services;
    }
}
