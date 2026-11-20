using System.Reflection;
using Microsoft.EntityFrameworkCore;
using soft.square.net.WebApi.Contracts.Tenancy;
using soft.square.net.WebApi.Data;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;

namespace soft.square.net.WebApi.Modules.ModuleName.Infrastructure.Data;

/// <summary>
/// The module's tenant data: schema "modulename" in each tenant's database (table prefix on engines without schemas).
/// The platform's <see cref="TenantDbContext"/> adds the tenant filter on every <c>ITenantOwned</c> entity, stamps the
/// tenant on save and dispatches domain events. The entity mappings are the <c>IEntityTypeConfiguration</c> classes in
/// <c>Data/Config</c>; the migrations, one set per engine, are in <c>Data/Migrations/&lt;Provider&gt;</c>.
/// </summary>
public sealed class ModuleNameDbContext(DbContextOptions<ModuleNameDbContext> options, ITenantContext tenant) : TenantDbContext(options, tenant)
{
    protected override string Schema => "modulename";

    public DbSet<Note> Notes => Set<Note>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
