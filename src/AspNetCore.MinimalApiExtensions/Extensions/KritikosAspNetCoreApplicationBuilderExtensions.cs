namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Kritikos.AspNetCore.MinimalApiExtensions.Middleware;

using Microsoft.AspNetCore.Builder;

public static class KritikosAspNetCoreApplicationBuilderExtensions
{
  public static IApplicationBuilder UseCorrelationHeader(this IApplicationBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    return app.UseMiddleware<CorrelationHeaderMiddleware>();
  }
}
