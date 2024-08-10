// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;
using Kritikos.AspNetCore.MinimalApiExtensions.Services;

public static partial class KritikosIEndpointExtensions
{
  public static IServiceCollection AddPeriodicBackgroundService<TService, TOptions>(
    this IServiceCollection services,
    string configurationSection = nameof(TOptions),
    Action<TOptions>? configureOptions = null)
    where TService : PeriodicBackgroundService<TService, TOptions>
    where TOptions : PeriodicBackgroundServiceOptions<TService>
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOptions<TOptions>()
      .Configure(configureOptions ?? (_ => { }));

    return new ServiceCollection()
      .AddHostedService<TService>();
  }
}
