using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using soft.square.net.WebApi.Contracts.Tenancy;
using soft.square.net.WebApi.Modules.ModuleName.Infrastructure.Data;

namespace soft.square.net.WebApi.Modules.ModuleName.IntegrationTests.Data;

/// <summary>
/// One in-memory SQLite database with the module's model (EnsureCreated, not the per-provider migrations: those are
/// checked with <c>dotnet ef migrations has-pending-model-changes</c>). Each <see cref="For"/> call is a new context,
/// as a new request would get, for the given tenant.
/// </summary>
public sealed class ModuleNameDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<ModuleNameDbContext> _options;

    public ModuleNameDatabase()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<ModuleNameDbContext>().UseSqlite(_connection).Options;
        using var context = For("acme");
        context.Database.EnsureCreated();
    }

    public ModuleNameDbContext For(string? tenantId) => new(_options, new TestTenant(tenantId));

    public void Dispose() => _connection.Dispose();

    private sealed class TestTenant(string? id) : ITenantContext
    {
        public TenantInfo? Tenant { get; } = id is null ? null : new TenantInfo(id, id, TenantStatus.Active, [], "", "");
        public string? ResolvedBy => "test";
    }
}
