// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.SwaggerFeatureManagementOptions;

public static class KritikosSwaggerFeatureManagerDependencyInjectionExtensions
{
  public static void AddSwaggerFeatureManagementDefaults(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.ConfigureOptions<SwaggerFeatureOptions>();
  }
}
