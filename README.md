# ModularBlazor.ModuleTemplate

The `dotnet new` template for new [ModularBlazor](https://github.com/soft-square-net/ModularBlazor) modules. A new module starts here, in its own repo, with
the layout the platform's installer expects and a playground to run it in.

> **Status: scaffold.** The template is written once the platform publishes its contract packages and container
> images, which it builds on.

## Planned use

```bash
dotnet new install ModularBlazor.ModuleTemplate
dotnet new platform-module -n Billing
```

## What a generated module contains

```
src/<Name>.Domain/ .Application/ .Infrastructure/   server part (Infrastructure is the entry assembly)
src/<Name>.Client/                                 typed API client, shipped as a hidden client module
src/Module.<Name>/                                 optional Blazor UI module
playground/                                        Aspire AppHost: platform containers + this module from a dev path
tests/
.github/workflows/                                 build, pack the .modpkg bundle, publish packages on a tag
```

## Layout of this repo

```
templates/platform-module/   the template content (.template.config/template.json + the files above)
```

## How it builds and ships

- It compiles against the platform's contract packages (`WebApi.Contracts`, `ModularBlazor.Abstractions`, ...) from the
  soft-square-net GitHub Packages feed. It never references platform source.
- CI packs one bundle per release: `<module>-<version>.modpkg`, a zip with `module.json`, `server/`, `client/` and
  `contracts/`. The platform's admin area installs it: server part first (tenant migrations), then the client part.
- One version for the whole bundle, set once from the git tag. Unchanged parts are skipped on install.
- An API client ships once, inside the bundle that owns the API, as a hidden client module. Modules that call it
  reference its package with `ExcludeAssets="runtime"` and declare a dependency with a version range in `module.json`.

## Developing

`playground/` will hold a small Aspire AppHost: the platform's containers (gateway, WebApi, identity, Blazor client)
plus this repo's projects, loaded from their build output, so F5 shows a change without packing or uploading.

## Branches

Trunk-based: `main` always produces an installable bundle; short-lived `feat/...` and `fix/...` branches; a release
is a tag such as `v1.2.0`.
