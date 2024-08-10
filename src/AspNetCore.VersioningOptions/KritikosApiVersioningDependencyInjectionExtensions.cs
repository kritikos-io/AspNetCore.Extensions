// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.VersioningOptions;

public static class KritikosApiVersioningDependencyInjectionExtensions
{
  /// <summary>
  /// Adds opinionated default options for api versioning.
  /// </summary>
  /// <param name="services"><see cref="IServiceCollection"/> to configure.</param>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> are null.</exception>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  public static IServiceCollection AddApiVersioningDefaults(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services
      .AddApiVersioning()
      .AddApiExplorer()
      .EnableApiVersionBinding();

    services.ConfigureOptions<ApiVersioningDefaultOptions>();

    return services;
  }
}
