namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Kritikos.AspNetCore.MinimalApiExtensions.Middleware;

using Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/> to add ASP.NET Core middleware.
/// </summary>
public static class KritikosAspNetCoreApplicationBuilderExtensions
{
  /// <summary>
  /// Adds the correlation header middleware to the application pipeline.
  /// </summary>
  /// <param name="app">The <see cref="IApplicationBuilder"/> to configure.</param>
  /// <returns>The configured <see cref="IApplicationBuilder"/>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
  public static IApplicationBuilder UseCorrelationHeader(this IApplicationBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    return app.UseMiddleware<CorrelationHeaderMiddleware>();
  }
}
