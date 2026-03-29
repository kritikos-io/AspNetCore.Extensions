namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="WebApplication"/> to map endpoints.
/// </summary>
public static class KritikosAspNetCoreWebApplicationExtensions
{
  /// <summary>
  /// Maps all endpoints defined in implementations of <see cref="IEndpoint"/>.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  /// <param name="route">An <see cref="IEndpointRouteBuilder"/> to handle common conventions for all endpoints.</param>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  public static WebApplication MapEndpoints(this WebApplication app, IEndpointRouteBuilder? route = null)
  {
    ArgumentNullException.ThrowIfNull(app);

    var mapping = route ?? app;
    var endpoints = app.Services.GetService<IEnumerable<IEndpoint>>() ?? [];
    foreach (var endpoint in endpoints)
    {
      endpoint.MapEndpoint(mapping);
    }

    return app;
  }
}
