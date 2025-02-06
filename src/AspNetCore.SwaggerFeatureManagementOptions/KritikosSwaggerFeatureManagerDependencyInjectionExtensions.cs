namespace Kritikos.AspNetCore.SwaggerFeatureManagementOptions;

using Microsoft.Extensions.DependencyInjection;

public static class KritikosSwaggerFeatureManagerDependencyInjectionExtensions
{
  public static void AddSwaggerFeatureManagementDefaults(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.ConfigureOptions<SwaggerFeatureOptions>();
  }
}
