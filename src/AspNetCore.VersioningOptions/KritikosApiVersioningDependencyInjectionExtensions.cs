namespace Kritikos.AspNetCore.VersioningOptions;

using Asp.Versioning;
using Asp.Versioning.Builder;

using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

/// <summary>
/// Dependency injection extensions for configuring API versioning with opinionated defaults.
/// </summary>
public static class KritikosApiVersioningDependencyInjectionExtensions
{
  /// <param name="services"><see cref="IServiceCollection"/> to configure.</param>
  extension(IServiceCollection services)
  {
    /// <summary>
    /// Adds opinionated API-versioning defaults and registers the shared <see cref="ApiVersionSet"/> and its
    /// <see cref="ApiVersionModel"/> (built from <paramref name="setupAction"/>) as singletons, letting versioned
    /// endpoints resolve them from services.
    /// </summary>
    /// <param name="setupAction">Configures the <see cref="ApiVersionSetBuilder"/> with the supported API versions.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="setupAction"/> are null.</exception>
    public IServiceCollection AddApiVersioningDefaults(Action<ApiVersionSetBuilder> setupAction)
    {
      ArgumentNullException.ThrowIfNull(services);
      ArgumentNullException.ThrowIfNull(setupAction);

      services
        .AddApiVersioning()
        .AddApiExplorer();

      services.ConfigureOptions<ApiVersioningDefaultOptions>();
      services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiCatalogSource, ApiVersionCatalogSource>());

      services.TryAddSingleton(_ =>
      {
        var set = new ApiVersionSetBuilder(name: null);
        setupAction(set);
        return set.Build();
      });

      services.TryAddSingleton(static provider =>
      {
        var set = provider.GetRequiredService<ApiVersionSet>();
        var options = provider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
        return set.Build(options);
      });

      return services;
    }
  }
}
