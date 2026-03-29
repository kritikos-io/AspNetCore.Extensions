namespace Kritikos.AspNetCore.OpenApiVersioningOptions;

using Asp.Versioning;

using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding versioned OpenAPI support to the service collection.
/// </summary>
public static class OpenApiVersioningExtensions
{
  /// <summary>
  /// Adds OpenAPI document generation for each API version defined in the version model provider.
  /// </summary>
  /// <typeparam name="TVersionModelProvider">The type providing the API version model.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
  /// <param name="configure">An optional action to further configure <see cref="OpenApiOptions"/>.</param>
  /// <param name="versionNameSelector">An optional function to customize the version group name.</param>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  public static IServiceCollection AddVersionedOpenApi<TVersionModelProvider>(
    this IServiceCollection services,
    Action<OpenApiOptions>? configure = null,
    Func<ApiVersion, string>? versionNameSelector = null)
    where TVersionModelProvider : IApiVersionModelProvider
  {
    ArgumentNullException.ThrowIfNull(services);

    foreach (var version in TVersionModelProvider.VersionModel.ImplementedApiVersions)
    {
      services.AddOpenApi($"v{version}", options =>
      {
        var versionName = versionNameSelector?.Invoke(version);
        options.ShouldInclude = v => string.IsNullOrWhiteSpace(v.GroupName) || v.GroupName == $"v{version}";
        configure?.Invoke(options);
      });
    }

    return services;
  }
}
