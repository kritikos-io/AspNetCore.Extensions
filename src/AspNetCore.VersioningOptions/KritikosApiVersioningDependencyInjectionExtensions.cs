namespace Kritikos.AspNetCore.VersioningOptions;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;

using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CA1034 // Do not nest type - false positive from C# 14 extension blocks
#pragma warning disable CA1708 // Identifiers should differ by more than case - false positive from C# 14 extension blocks

/// <summary>
/// Dependency injection extensions for configuring API versioning with opinionated defaults.
/// </summary>
public static class KritikosApiVersioningDependencyInjectionExtensions
{
  /// <param name="services"><see cref="IServiceCollection"/> to configure.</param>
  extension(IServiceCollection services)
  {
    /// <summary>
    /// Adds opinionated default options for api versioning.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> are null.</exception>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    public IServiceCollection AddApiVersioningDefaults()
    {
      ArgumentNullException.ThrowIfNull(services);

      services
        .AddApiVersioning()
        .AddApiExplorer()
        .EnableApiVersionBinding();

      services.ConfigureOptions<ApiVersioningDefaultOptions>();

      return services;
    }

    /// <summary>
    /// Registers an <see cref="IApiVersionModelProvider"/> and adds its <see cref="ApiVersionModel"/> as a singleton.
    /// </summary>
    /// <typeparam name="TVersionModelProvider">The type implementing <see cref="IApiVersionModelProvider"/>.</typeparam>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    public IServiceCollection AddApiVersionModelProvider<TVersionModelProvider>()
      where TVersionModelProvider : IApiVersionModelProvider
    {
      ArgumentNullException.ThrowIfNull(services);
      services.AddSingleton(TVersionModelProvider.VersionModel);
      return services;
    }

    /// <summary>
    /// Registers an <see cref="IApiVersionSetProvider"/> and adds its <see cref="ApiVersionSet"/> as a singleton.
    /// </summary>
    /// <typeparam name="TVersionSetProvider">The type implementing <see cref="IApiVersionSetProvider"/>.</typeparam>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    public IServiceCollection AddApiVersionSetProvider<TVersionSetProvider>()
      where TVersionSetProvider : IApiVersionSetProvider
    {
      ArgumentNullException.ThrowIfNull(services);
      services.AddSingleton<ApiVersionSet>(static _ => TVersionSetProvider.VersionSet);
      return services;
    }
  }

  extension(WebApplication app)
  {
    /// <summary>
    /// Builds and assigns the <see cref="ApiVersionSet"/> from the registered version model, applying optional configuration.
    /// </summary>
    /// <typeparam name="TVersionSetProvider">The type implementing <see cref="IApiVersionSetProvider"/>.</typeparam>
    /// <param name="setupAction">An optional action to further configure the <see cref="ApiVersionSetBuilder"/>.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public WebApplication AddApiVersionSet<TVersionSetProvider>(
      Action<ApiVersionSetBuilder>? setupAction = null)
      where TVersionSetProvider : IApiVersionSetProvider
    {
      ArgumentNullException.ThrowIfNull(app);
      var versionModel = app.Services.GetRequiredService<ApiVersionModel>();

      var set = app.NewApiVersionSet()
        .HasApiVersions(versionModel.SupportedApiVersions)
        .HasDeprecatedApiVersions(versionModel.DeprecatedApiVersions);

      setupAction?.Invoke(set);
      TVersionSetProvider.VersionSet = set.Build();

      return app;
    }
  }
}
