// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.MinimalApiExtensions.Middleware;
using Kritikos.AspNetCore.MinimalApiExtensions.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class KritikosCorrelationHeaderExtensions
{
  public static IApplicationBuilder UseCorrelationHeader(this IApplicationBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    return app.UseMiddleware<CorrelationHeaderMiddleware>();
  }

  public static IServiceCollection AddCorrelationHeader(this IServiceCollection services, Action<CorrelationHeaderOptions>? configure = null)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOptionsDefinition<CorrelationHeaderOptions>();
    services.Configure(configure ?? (static _ => { }));
    services.TryAddSingleton<CorrelationHeaderMiddleware>();

    return services;
  }
}
