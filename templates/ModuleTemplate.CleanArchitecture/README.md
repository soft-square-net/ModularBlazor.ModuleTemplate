# ModularBlazor.Module.ModuleName

ModuleName: a runtime module of the [ModularBlazor](https://github.com/soft-square-net/ModularBlazor.Platform)
platform, in [Ardalis Clean Architecture](https://github.com/ardalis/CleanArchitecture) layers. It was generated from
the `platform-module-ca` template, with a `Note` aggregate as the example feature: replace it with your own.

## Layout

```
src/WebApi.Modules.ModuleName.Core/             Core: aggregates (NoteAggregate), their rules and specifications
src/WebApi.Modules.ModuleName.UseCases/         use cases: one folder each, a Mediator command or query and its handler, returning an Ardalis Result
src/WebApi.Modules.ModuleName.Infrastructure/   data: ModuleNameDbContext, entity configurations, tenant migrations per database provider
src/WebApi.Modules.ModuleName.Web/              entry assembly: IRuntimeModule, the Carter route table, one REPR endpoint class per route
tests/         UnitTests (Core, UseCases; NSubstitute) and IntegrationTests (Infrastructure on SQLite)
playground/    Aspire AppHost: the platform (from a Platform checkout) with this module, see "Developing"
docs/          clean-architecture.md: layers, dependency rules, adding a feature
```

The module's id, route prefix and database schema are `modulename`; its permissions are `modulename.read` and
`modulename.write`.

## The example: notes

| Method and route (under `/api/modulename`) | Permission | Use case |
| --- | --- | --- |
| `GET /notes?page=&pageSize=` | `modulename.read` | `ListNotesQuery`: one page, newest first, with the total |
| `GET /notes/search?text=&page=&pageSize=` | `modulename.read` | `SearchNotesQuery`: title or body contains the text, ignoring case |
| `GET /notes/{id}` | `modulename.read` | `GetNoteByIdQuery` |
| `POST /notes` `{ title, body }` | `modulename.write` | `CreateNoteCommand`: 201 with the note |
| `PUT /notes/{id}` `{ version, title, body }` | `modulename.write` | `EditNoteCommand`: 409 if the note changed since `version` |
| `DELETE /notes/{id}?version=` | `modulename.write` | `DeleteNoteCommand`: 204; 409 if the note changed since `version` |

Every call needs a signed-in caller in a tenant (`X-Tenant` header or the token's tenant). The setting
`ModuleName:MaxPageSize` (default 100) caps the page size; change it on the Host Modules page.

## Building

```bash
dotnet build
```

```bash
dotnet test
```

The platform packages come from the soft-square-net GitHub Packages feed, which needs a token to restore even though
the packages are public: create a personal access token (classic) with `read:packages` and put it in the environment
as `GITHUB_PACKAGES_TOKEN` (`NuGet.Config` reads it). The platform version is pinned once, in `global.json`.

Tools that need the platform's source run from a checkout of
[ModularBlazor.Platform](https://github.com/soft-square-net/ModularBlazor.Platform). Add a migration for every
database provider after a model change:

```powershell
./src/WebApi/scripts/Add-Migration.ps1 -Project <this repo>/src/WebApi.Modules.ModuleName.Infrastructure -Name <Name> `
    -MigrationsDir Data/Migrations -Namespace soft.square.net.WebApi.Modules.ModuleName.Infrastructure.Data.Migrations
```

Export the module's OpenAPI document (the entry project is `Web`), for example to generate a typed client:

```powershell
./src/WebApi/scripts/Export-ModuleOpenApi.ps1 -Project <this repo>/src/WebApi.Modules.ModuleName.Web -ModuleId modulename
```

## Installing

The server part is the build output of `WebApi.Modules.ModuleName.Web` (`bin/<Configuration>/net10.0`: every layer
plus the private Mediator and Ardalis.Result). Zip it and upload it on the client's Host Modules page
(`/host-modules`, as a platform admin), then activate it: the tenant migrations run per tenant, the routes are mapped
under `/api/modulename`. The playground's `install` profile builds that zip for you (`playground/out/modulename.zip`).

## Developing

`playground/` is a small Aspire AppHost: PostgreSQL, the identity service, one WebApi and the Blazor client, with this
module in it. It builds the platform from source, from a
[ModularBlazor.Platform](https://github.com/soft-square-net/ModularBlazor.Platform) checkout next to this repo:

```
<workspace>/Platform               git clone https://github.com/soft-square-net/ModularBlazor.Platform Platform
<workspace>/Module.ModuleName      this repo
```

(another location: `--property:PlatformRoot=<path>/`). It uses the platform lab's ports (identity 5090, API 5080,
client 5070), so stop the lab and any other playground first.

```bash
dotnet run --project playground --launch-profile dev
```

| Profile | What it does |
| --- | --- |
| `dev` (default) | The WebApi loads the module straight from `src/WebApi.Modules.ModuleName.Web/bin/<Configuration>/net10.0` (`Modules:Local`, Development only): installed and active on start. Change code, rebuild, restart the `webapi` resource in the dashboard |
| `install` | The platform starts without the module, as in production; upload `playground/out/modulename.zip` on http://localhost:5070/host-modules (sign in as the dev platform admin) and activate it |

In Development the platform seeds the tenants `acme` and `globex`. The playground grants the module's permissions in
`acme` to the dev accounts through `Access:DevGrants`: `acme-admin@modular.local` gets `modulename.*`,
`editor@modular.local` gets `modulename.read`. Their passwords are in the platform's
`WebApi.Identity.Server/appsettings.Development.json`. The API is browsable at http://localhost:5080/scalar.

Data stays in the Docker volume `modulename-playground-data` and in `playground/.data`; delete both to start over.

## Branches

Trunk-based: `main` always builds an installable module; short-lived `feat/...` and `fix/...` branches; a release is
a tag such as `v1.2.0`.
