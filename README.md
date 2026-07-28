# AspNetCore.Extensions

A collection of small, single-purpose, opinionated libraries that bootstrap and harden ASP.NET Core applications and fill common gaps in the base class library.

Each library targets `net10.0`, is independently useful, and ships as a `Kritikos.<Project>` NuGet package built with the same governance (nullable, all analyzers, code style enforced at build, deterministic builds, SBOM, and SourceLink). Take only the pieces you need.

## Getting started

Install the package for the capability you want; every identifier is prefixed with `Kritikos.`:

```sh
dotnet add package Kritikos.AspNetCore.MinimalApiExtensions
```

> [!NOTE]
> Packages resolve from nuget.org and from the private `kritikos.io` feed configured in [nuget.config](nuget.config). The [PetStore.WebApi](samples/PetStore.WebApi) sample shows several of these libraries working together.

## Libraries at a glance

| Package | Summary |
| --- | --- |
| [Extensions.Options](src/Extensions.Options) | Static-abstract contracts that let an options class declare its own configuration section. |
| [Extensions.Options.DependencyInjection](src/Extensions.Options.DependencyInjection) | Binds and validates those options definitions and validates them on startup. |
| [AspNetCore.MinimalApiExtensions](src/AspNetCore.MinimalApiExtensions) | Startup, endpoint-mapping, correlation, [Heartbeat](#heartbeat) liveness, and periodic background-service building blocks for Minimal APIs. |
| [AspNetCore.VersioningOptions](src/AspNetCore.VersioningOptions) | Opinionated defaults for `Asp.Versioning` API versioning and the API explorer. |
| [AspNetCore.FeatureManagementOptions](src/AspNetCore.FeatureManagementOptions) | Feature-flag endpoint filters and a session-backed feature manager for Minimal APIs. |
| [AspNetCore.OpenApiExtensions](src/AspNetCore.OpenApiExtensions) | OpenAPI operation transformer that documents auth responses and security for `[Authorize]` endpoints. |
| [AspNetCore.OpenApiOidcExtensions](src/AspNetCore.OpenApiOidcExtensions) | OpenAPI document transformer that adds an OpenID Connect / OAuth2 scheme from a discovery document. |
| [AspNetCore.OpenApiFeatureManagementOptions](src/AspNetCore.OpenApiFeatureManagementOptions) | OpenAPI document transformer that hides operations behind disabled feature flags. |
| [HttpClient.Handlers](src/HttpClient.Handlers) | Delegating handler that injects a randomized `User-Agent` when one is absent. |
| [HttpClient.AuthenticationHandlers](src/HttpClient.AuthenticationHandlers) | Delegating handler that attaches an OAuth2 client-credentials bearer token, cached until near expiry. |
| [PagedResults](src/PagedResults) | Generic offset-pagination result types and EF Core query helpers. |
| [SemanticVersioning](src/SemanticVersioning) | SemVer 2.0.0 parsing and comparison records. |
| [SemanticVersioning.DependencyInjection](src/SemanticVersioning.DependencyInjection) | Registers a `SemanticVersionDescriptor` parsed from an assembly's informational version. |

### Dependency graph

Arrows point from a library to the in-repo library it depends on. `PagedResults`, `HttpClient.Handlers`, and `HttpClient.AuthenticationHandlers` have no in-repo dependencies and are omitted.

```mermaid
flowchart TD
  eodi[Extensions.Options.DependencyInjection] --> eo[Extensions.Options]
  mae[AspNetCore.MinimalApiExtensions] --> eodi
  oae[AspNetCore.OpenApiExtensions] --> mae
  vo[AspNetCore.VersioningOptions] --> mae
  ooie[AspNetCore.OpenApiOidcExtensions] --> oae
  ofmo[AspNetCore.OpenApiFeatureManagementOptions] --> oae
  ofmo --> fmo[AspNetCore.FeatureManagementOptions]
  svdi[SemanticVersioning.DependencyInjection] --> sv[SemanticVersioning]
```

## Foundation

### Extensions.Options

Contracts that let an options class declare where it binds from, so the section location lives with the type instead of at every call site.

- Key types: `IOptionsDefinition` (`static abstract string Location`) and `INamedOptionsDefinition` (`static abstract Dictionary<string, string> NamedLocations`).
- Registration: none on its own — pair it with `Extensions.Options.DependencyInjection`.

```csharp
public sealed class WeatherOptions : IOptionsDefinition
{
  public static string Location => "Weather";

  public Uri Endpoint { get; set; } = new("https://example.invalid");
}
```

### Extensions.Options.DependencyInjection

Binds an `IOptionsDefinition` / `INamedOptionsDefinition` to configuration, applies DataAnnotations validation, and validates it on startup so misconfiguration fails fast.

- Registration: `services.AddOptionsDefinition<WeatherOptions>()`, or scan an assembly with `services.AddOptionsDefinitions(typeof(WeatherOptions).Assembly)` (use `AddNamedOptionsDefinitions` for `INamedOptionsDefinition`).
- Depends on: `Extensions.Options`.

```csharp
services.AddOptionsDefinition<WeatherOptions>();
```

## ASP.NET Core

### AspNetCore.MinimalApiExtensions

The core toolkit for Minimal-API hosts: correlation-header middleware, the [Heartbeat](#heartbeat) liveness subsystem, a periodic background-service base, endpoint-mapping contracts, and startup abstractions.

- Endpoints: implement `IEndpoint` and call `app.MapEndpoints()` to discover and register them.
- Correlation: `services.AddCorrelationHeader()` plus `app.UseCorrelationHeader()`; binds `AspNetCore:Middleware:Correlation` (`Header`, `IncludeInResponse`).
- Heartbeat: `services.AddHeartbeat()`; binds `AspNetCore:Heartbeat`. Detailed below.
- Periodic work: derive an options type from `PeriodicBackgroundServiceOptions` (`Interval`, `TriggerStopsCurrentExecution`) and register with `services.AddPeriodicBackgroundService<TService, TOptions>()`.
- Startup: `IApplicationStartup` and `IWebApplicationStartup` compose registration and pipeline setup.
- Depends on: `Extensions.Options.DependencyInjection` and the ASP.NET Core shared framework.

#### Heartbeat

A liveness heartbeat registry, watchdog, and health check that detects and aborts stalled long-running work: a service registers a named `Heartbeat`, beats it as work progresses, and links its `Token` into cancellable calls. A background watchdog cancels any source whose beat goes stale, and an aggregate health check tagged `live` reports a stale source as unhealthy so the liveness probe restarts a wedged host; a process with no registered sources stays healthy.

```csharp
services.AddHeartbeat();

// HeartbeatRegistry is injected into the worker.
using var heartbeat = registry.Register("import", TimeSpan.FromSeconds(30));
while (moreWork)
{
  await StepAsync(heartbeat.Token);
  heartbeat.Beat();
}
```

`HeartbeatOptions` binds from `AspNetCore:Heartbeat` and is validated on startup.

| Property | Default | Notes |
| --- | --- | --- |
| `DefaultTimeout` | 15 seconds | Staleness window for sources that register without their own. Must be greater than zero. |
| `WatchdogInterval` | 10 seconds | How often the watchdog scans for stale sources. Must be greater than zero; changes apply live. |

Staleness is measured with a monotonic `TimeProvider` timestamp, so NTP corrections, DST shifts, and VM pause and resume never skew it. Each source is cancelled inside its own `try`/`catch`, so one faulting linked-token callback cannot fault the watchdog or bring down the host, and re-registering a name disposes the previous handle. Metrics and traces are emitted under the source name `Kritikos.AspNetCore.MinimalApiExtensions.Heartbeat` for wiring into OpenTelemetry.

### AspNetCore.VersioningOptions

Opinionated defaults for [Asp.Versioning][asp-versioning]: `ApiVersioningDefaultOptions` configures both `ApiVersioningOptions` and `ApiExplorerOptions` via `IConfigureOptions<T>`.

- Registration: `services.AddApiVersioningDefaults()`; supply a custom version model with `services.AddApiVersionModelProvider<TProvider>()` (a `IApiVersionModelProvider`).
- Depends on: `AspNetCore.MinimalApiExtensions` and `Asp.Versioning.Mvc.ApiExplorer`.

### AspNetCore.FeatureManagementOptions

Gates Minimal-API endpoints on [Microsoft.FeatureManagement][feature-management] flags and adds a session-backed feature-state manager.

- Key types: `FeatureGateEndpointFilter` (`IEndpointFilter`), the `FeatureEndpointFilterExtensions` endpoint-filter helpers, `SessionFeatureManager` (`ISessionManager`), and the `RequirementType` enum (`All` / `Any`).
- Registration: `services.AddFeatureManagementSessionManager()`, then apply gates through the endpoint-filter extensions.
- Depends on: `Microsoft.FeatureManagement.AspNetCore`.

## OpenAPI

These transformers extend the built-in `Microsoft.AspNetCore.OpenApi` pipeline; register them on `OpenApiOptions` with `options.AddOperationTransformer<T>()` or `options.AddDocumentTransformer<T>()`.

### AspNetCore.OpenApiExtensions

`AuthorizationCheckOperationTransformer` (`IOpenApiOperationTransformer`) adds `401` and `403` responses and an OAuth2 security requirement to every operation behind `[Authorize]`, keeping the spec honest about what needs a token.

- Depends on: `AspNetCore.MinimalApiExtensions` and `Microsoft.AspNetCore.OpenApi`.

### AspNetCore.OpenApiOidcExtensions

`OidcSecuritySchemeTransformer<TOptions>` (`IOpenApiDocumentTransformer`) fetches the OpenID Connect discovery document from the configured authority at document-generation time and adds the matching OAuth2 / OpenID Connect security scheme.

- Configuration: subclass the abstract `OpenApiOpenIdOptions` (with its `Authority`) and bind it as `IOptions<TOptions>`.
- Depends on: `AspNetCore.OpenApiExtensions` and `Microsoft.AspNetCore.Authentication.OpenIdConnect`.

### AspNetCore.OpenApiFeatureManagementOptions

`FeatureFilterDocumentTransformer` (`IOpenApiDocumentTransformer`) removes operations whose feature gate is disabled from the generated document, so the published spec matches runtime behaviour.

- Depends on: `AspNetCore.FeatureManagementOptions` and `AspNetCore.OpenApiExtensions`.

## HTTP client

### HttpClient.Handlers

`UserAgentHandler` (`DelegatingHandler`) sets a randomized `User-Agent` — weighted by browser category — on any request that does not already carry one. The values come from `UserAgentProvider` / `IUserAgentProvider` and are configured through `UserAgentProviderOptions` (`Agents`, `BrowserWeights`).

- Registration: add it to a typed client with `AddHttpMessageHandler`.

### HttpClient.AuthenticationHandlers

`OpenIdConnectClientCredentialsHandler` (`DelegatingHandler`) attaches an OAuth2 [client-credentials][client-credentials] bearer token obtained by `OpenIdConnectTokenProvider`, which caches the token until shortly before it expires and coalesces concurrent refreshes into a single request to the identity provider.

- Configuration: `OpenIdConnectHandlerOptions` (`WellKnownEndpoint`, `ClientId`, `ClientSecret`); the provider also requires a `TimeProvider`.
- Depends on: `Microsoft.Extensions.Http` and `Microsoft.IdentityModel.Protocols.OpenIdConnect`.

> [!IMPORTANT]
> Register `OpenIdConnectTokenProvider` as a singleton so the token cache is shared across the pooled handler instances, and make sure a `TimeProvider` is registered (for example `services.TryAddSingleton(TimeProvider.System)`).

## General purpose

### PagedResults

Generic offset-pagination types — `PagedResult<T>` and the sealed `OffsetPagedResult<T>` — with `PagedResultExtensions` and `OffsetPagedResultExtensions` to materialize an EF Core `IQueryable` into a page plus total-count metadata.

- Depends on: `Microsoft.EntityFrameworkCore`.

### SemanticVersioning

[SemVer 2.0.0][semver] value types: the comparable `SemanticVersionDescriptor` record, `PreReleaseMetadataDescriptor`, `BuildMetadataDescriptor`, and `SemanticVersioningConstants` for parsing and ordering versions.

### SemanticVersioning.DependencyInjection

Registers a `SemanticVersionDescriptor` parsed from an assembly's `AssemblyInformationalVersionAttribute` so components can inject the running version.

- Registration: `services.AddSemanticVersionDescriptor(assembly)` or `services.AddSemanticVersionDescriptor(typeof(T))` (resolved from the type's assembly).
- Depends on: `SemanticVersioning`.

## Sample

The [PetStore.WebApi](samples/PetStore.WebApi) sample assembles the Minimal-API startup, versioning defaults, feature-gated endpoints, and the OpenAPI transformers (authorization, OIDC, and versioning) into one runnable API.

## Repository conventions

- All projects target `net10.0` with nullable reference types, the `All` analyzer mode, and code style enforced at build.
- Package versions are centrally managed in [Directory.Packages.props](Directory.Packages.props); builds are deterministic and produce lock files.
- Packaging prefers a project-local `README.md` and falls back to this file, so per-project READMEs are the preferred place for library-specific detail.

## License

Licensed under the terms of [LICENSE.md](LICENSE.md).

[asp-versioning]: https://github.com/dotnet/aspnet-api-versioning
[feature-management]: https://learn.microsoft.com/azure/azure-app-configuration/feature-management-dotnet-reference
[client-credentials]: https://datatracker.ietf.org/doc/html/rfc6749#section-4.4
[semver]: https://semver.org/spec/v2.0.0.html
