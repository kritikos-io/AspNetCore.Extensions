namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class KritikosAspNetCoreWebApplicationExtensions
{
  /// <summary>
  /// Maps all endpoints defined in implementations of <see cref="IEndpoint"/>.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  public static WebApplication MapEndpoints(this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);

    var endpoints = app.Services.GetService<IEnumerable<IEndpoint>>() ?? [];
    foreach (var endpoint in endpoints)
    {
      endpoint.MapEndpoint(app);
    }

    return app;
  }
}
