# Clean Architecture in this module

This module follows [Ardalis Clean Architecture](https://github.com/ardalis/CleanArchitecture), adapted to what a
ModularBlazor runtime module is: an assembly set that the host loads into a collectible load context, maps under
`/api/modulename`, and can unload again. The layout comes from the platform's Content module.

## The layers

```
src/
  WebApi.Modules.ModuleName.Core/            Core       (ModuleLayer Domain)
    NoteAggregate/Note.cs                    the aggregate: state, rules (DomainException), private setters
    NoteAggregate/Specifications/            NoteByIdSpec, NotesPageSpec, NoteSearchSpec
  WebApi.Modules.ModuleName.UseCases/        UseCases   (ModuleLayer Application)
    ModuleNamePermissions.cs ModuleNameOptions.cs ResultExtensions.cs
    Behaviors/LoggingBehavior.cs             Mediator pipeline behaviour
    Notes/NoteDto.cs                         DTOs and helpers the note use cases share
    Notes/Create/CreateNote.cs               CreateNoteCommand and CreateNoteHandler
    Notes/Edit/  Notes/Delete/  Notes/GetById/  Notes/List/  Notes/Search/
  WebApi.Modules.ModuleName.Infrastructure/  Infrastructure
    Data/ModuleNameDbContext.cs              a TenantDbContext (schema "modulename", tenant filter, domain events)
    Data/Config/NoteConfiguration.cs         one IEntityTypeConfiguration per entity
    Data/Migrations/<Provider>/              one migration set per database engine
    InfrastructureServiceExtensions.cs       AddInfrastructureServices()
  WebApi.Modules.ModuleName.Web/             Web: the entry assembly and composition root (ModuleLayer Module)
    ModuleNameModule.cs                      IRuntimeModule: manifest, permissions, settings, ConfigureServices
    Configurations/                          ServiceConfigs (all registrations), MediatorConfig
    ModuleNameRoutes.cs                      the only ICarterModule: the route table
    Notes/                                   one endpoint class per route (REPR)
    Extensions/                              ResultExtensions (Result -> HTTP), ModuleNameProblemsFilter
tests/
  WebApi.Modules.ModuleName.UnitTests/       Core rules; UseCases handlers with NSubstitute
  WebApi.Modules.ModuleName.IntegrationTests/ the DbContext, mapping and specifications on SQLite
```

| Layer | Holds | Knows about |
| --- | --- | --- |
| Core | Aggregates and their rules (throwing `DomainException`), entity specifications | the platform's SharedKernel (`AggregateRoot`, `IRepository`) and Ardalis.Specification |
| UseCases | One folder per use case: a command or query and its handler, returning an `Ardalis.Result`; DTOs; permissions; settings | Core; the platform's contracts (`IAccessScopes`, `IGrantStore`); Mediator.Abstractions |
| Infrastructure | EF Core: the DbContext, mappings, migrations | Core; the platform's data layer (`TenantDbContext`, `EfRepository`) |
| Web | The module entry, registrations, routes, endpoints, HTTP mapping of results | everything above; ASP.NET Core, Carter, the Mediator source generator |

## Dependency rules

```
Core  <-  UseCases  <-  Web
  ^                      |
  +---  Infrastructure <-+
```

- Core references nothing of the module. UseCases references Core only. Infrastructure references Core only.
- Web references UseCases and Infrastructure and is the only place that wires them together (`ServiceConfigs`).
- UseCases never touches EF Core: handlers get `IRepository<T>` / `IReadRepository<T>` (the platform registers EF
  repositories for them) and query with specifications from Core.
- Endpoints contain no logic: bind the request, `ISender.Send` a command or query, map the `Result` to HTTP.
- Repositories save immediately (`AddAsync`, `UpdateAsync`, `DeleteAsync`); there is no unit of work. Domain events
  (raised with `Raise(...)` in an aggregate, handled by an `IDomainEventHandler<T>`) go through the platform's
  `TenantDbContext` dispatcher in the same save, not through the mediator.
- Failures a caller should see are `Result` statuses: `NotFound` (404), `Forbidden` (403), `Unauthorized` (401),
  `Conflict` (409), `Invalid` (400). A broken domain rule is a `DomainException`; `ModuleNameProblemsFilter` turns it,
  and EF concurrency conflicts, into 409 problems.
- Optimistic concurrency: an aggregate that callers edit carries a `Version` (a concurrency token); edits and deletes
  send back the version they read.

### Rules that come from the platform

- **No module type in a host-wide cache.** Do not inject `ILogger<T>` with a module type `T` (the host's logger
  factory caches by type): take `ILoggerFactory` and create the logger by name, as `LoggingBehavior` does. Register
  module services in `ConfigureServices` (the module's own container), never in the host's.
- **Host-shared packages are compile-only.** The Module SDK references WebApi.Contracts, SharedKernel, Data, EF Core,
  Ardalis.Specification and Carter with `ExcludeAssets=runtime`; the host provides them. Never ship a copy.
- **Private packages are fine when they keep no host-side state.** Mediator (source-generated: its handler table is
  code in the Web assembly) and Ardalis.Result are copied into the module output and unload with it. Avoid packages
  the host already ships (FluentValidation, for example): the module would share the host's copy and its caches.
- **Only the entry assembly is scanned.** The runtime maps the `ICarterModule` types of the assembly that holds the
  `IRuntimeModule`, which is Web, so endpoints live there.
- **Check that it unloads.** After the module has served requests, `POST /api/admin/modules/modulename/unload` and
  then `GET /api/admin/modules/unloading` should come back empty within a few seconds.

## Adding a feature

A new action on notes, say "pin":

1. **Core**: the rule on the aggregate, `Note.Pin(now)`, throwing `DomainException` when not allowed. A new query
   shape is a specification in `NoteAggregate/Specifications`.
2. **UseCases**: a folder `Notes/Pin/` with `PinNoteCommand` (`ICommand<Result<NoteDto>>`) and `PinNoteHandler`.
   Add a permission to `ModuleNamePermissions` if it needs a new one.
3. **Web**: `Notes/Pin.cs`, an `IEndpoint` with a static `HandleAsync` that sends the command and returns
   `Results<Ok<T>, ProblemHttpResult>`; then one line in `ModuleNameRoutes`. Name it `ModuleName_PinNote`: generated
   clients take their method names from it.
4. **Infrastructure**, if the model changed: the mapping in `Data/Config`, then a migration for every provider (see
   the README) and `dotnet ef migrations has-pending-model-changes` per provider.
5. **Client**: export the OpenAPI document again (README, "Building") and build: `ModuleNameClient` gains
   `PinNoteAsync`. Use it from a page in `Module.ModuleName` (pages inherit `ModuleNamePageBase`: `Api`, `RunAsync`,
   readable errors).
6. **Tests**: the rule in `UnitTests/Core`, the handler in `UnitTests/UseCases`, a new mapping or specification in
   `IntegrationTests`.

A new aggregate gets its own `<Name>Aggregate` folder in Core, a resource folder in UseCases and in Web, a `DbSet`
and a configuration in Infrastructure, and a migration.

Handlers are found by the Mediator source generator at compile time; there is nothing to register. Services that
several handlers share are registered in `ServiceConfigs`.

## Replacing the example

Delete `Core/NoteAggregate`, `UseCases/Notes`, `Web/Notes`, `Infrastructure/Data/Config/NoteConfiguration.cs`, the
`Notes` DbSet, the migrations and the note tests; add your aggregate the way the notes are built, update the
permissions, the manifest's description and settings, and add an initial migration per provider.
