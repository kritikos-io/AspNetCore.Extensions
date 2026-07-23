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
  /// <param name="route">An optional <see cref="IEndpointRouteBuilder"/> to map the endpoints onto; defaults to <paramref name="app"/> when not supplied.</param>
  /// <returns>The configured <see cref="WebApplication"/>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
  public static WebApplication MapEndpoints(this WebApplication app, IEndpointRouteBuilder? route = null)
  {
    ArgumentNullException.ThrowIfNull(app);

    var mapping = route ?? app;
    var endpoints = app.Services.GetServices<IEndpoint>();
    foreach (var endpoint in endpoints)
    {
      endpoint.MapEndpoint(mapping);
    }

    return app;
  }
}
