namespace Kritikos.AspNetCore.OpenApiVersioningOptions;

using Asp.Versioning;

using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

public static class OpenApiVersioningExtensions
{
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
