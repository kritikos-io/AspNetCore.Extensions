namespace Kritikos.AspNetCore.VersioningOptions;

using Microsoft.Extensions.DependencyInjection;

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
