using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using soft.square.net.WebApi.Contracts;
using soft.square.net.WebApi.Modules.ModuleName.Web.Configurations;

namespace soft.square.net.WebApi.Modules.ModuleName.Web;

/// <summary>
/// The module: its manifest (id, name, version, route prefix, default settings, permissions) and its services.
/// This is the composition root (the Ardalis template's Program.cs): the runtime creates it, reads the manifest and
/// calls <see cref="ConfigureServices"/> on the module's own container; the endpoints are in
/// <see cref="ModuleNameRoutes"/>, mapped under <c>/api/modulename</c>. Tenant data, schema "modulename".
/// </summary>
public sealed class ModuleNameModule : IRuntimeModule
{
    public ModuleManifest Manifest { get; } = new("modulename", "ModuleName", "0.1.0", "modulename")
    {
        Description = "ModuleName: notes per tenant (the template's example; replace with the module's own features).",
        DefaultSettings = new Dictionary<string, string?> { ["ModuleName:MaxPageSize"] = "100" },
        Permissions = ModuleNamePermissions.All
    };

    public void ConfigureServices(IServiceCollection services, IConfiguration settings) => services.AddServiceConfigs(settings);
}
