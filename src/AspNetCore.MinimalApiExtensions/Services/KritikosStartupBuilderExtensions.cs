// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;
using Kritikos.AspNetCore.MinimalApiExtensions.Services;

public static partial class KritikosStartupBuilderExtensions
{
  /// <summary>
  /// Registers a background service that runs periodically.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to register the service to.</param>
  /// <typeparam name="TService">The type of <see cref="PeriodicBackgroundService{TService,TOptions}"/> to register.</typeparam>
  /// <typeparam name="TOptions">The type of <see cref="PeriodicBackgroundServiceOptions{T}"/> to use in configuring the <see cref="PeriodicBackgroundService{TService,TOptions}"/>.</typeparam>
  /// <returns>A <see cref="IServiceCollection"/> containing <see cref="PeriodicBackgroundService{TService,TOptions}"/>.</returns>
  /// <remarks>Ensure there is a configured options class registered as <typeparamref name="TOptions"/> otherwise you will get a runtime <see cref="InvalidOperationException"/>.</remarks>
  public static IServiceCollection AddPeriodicBackgroundService<TService, TOptions>(this IServiceCollection services)
      where TService : PeriodicBackgroundService<TService, TOptions>
      where TOptions : PeriodicBackgroundServiceOptions<TService>
  {
    ArgumentNullException.ThrowIfNull(services);

    return services.AddHostedService<TService>();
  }
}
