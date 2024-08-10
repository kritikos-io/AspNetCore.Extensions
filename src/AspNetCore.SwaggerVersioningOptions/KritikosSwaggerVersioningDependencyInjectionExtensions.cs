// ReSharper disable InconsistentNaming : Following original method casing
// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using System.Globalization;

using Kritikos.AspNetCore.SwaggerVersioningOptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;

public static class KritikosSwaggerVersioningDependencyInjectionExtensions
{
  public static void AddSwaggerVersioning(this IServiceCollection services, OpenApiInfo apiInfo)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(apiInfo);

    services.TryAddSingleton(apiInfo);

    services.ConfigureOptions<SwaggerGenApiVersioningOptions>();
  }

  public static void AddRedocDefaults(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.ConfigureOptions<ReDocDefaultOptions>();
  }

  /// <summary>
  /// Configures Swagger UI to display all versions.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  /// <remarks>If you use Minimal Endpoints, they must be mapped <b>before</b> calling this method, otherwise only the default version will be configured.</remarks>
  public static void UseVersionedSwaggerUI(this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);

    app.UseSwaggerUI(
        options =>
        {
          var descriptions = app.DescribeApiVersions();
          foreach (var description in descriptions)
          {
            options.SwaggerEndpoint(
                $"/swagger/v{description.ApiVersion:VV}/swagger.json",
                description.ApiVersion.ToString("VVV", CultureInfo.InvariantCulture));
          }
        });
  }

  public static void UseVersionedReDoc(this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);
    foreach (var description in app.DescribeApiVersions())
    {
      app.UseReDoc(
          options =>
          {
            options.RoutePrefix = $"api/v{description.ApiVersion:VV}/docs";
            options.SpecUrl = $"/swagger/v{description.ApiVersion:VV}/swagger.json";
            options.DocumentTitle = $"{app.Environment.ApplicationName} v{description.ApiVersion:VV}";
          });
    }
  }
}
