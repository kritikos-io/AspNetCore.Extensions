namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;
using Kritikos.AspNetCore.MinimalApiExtensions.Middleware;
using Kritikos.AspNetCore.MinimalApiExtensions.Options;
using Kritikos.AspNetCore.MinimalApiExtensions.Services;
using Kritikos.Extensions.Options.Contracts;
using Kritikos.Extensions.Options.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class KritikosAspNetCoreDependencyInjectionsExtensions
{
  /// <summary>
  /// Adds services required for using correlation headers in the application.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to register the service to.</param>
  /// <param name="configure">An <see cref="System.Action{T}" /> to configure the provided <see cref="CorrelationHeaderOptions" />.</param>
  /// <returns></returns>
  public static IServiceCollection AddCorrelationHeader(this IServiceCollection services,
    Action<CorrelationHeaderOptions>? configure = null)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOptionsDefinition<CorrelationHeaderOptions>();
    services.Configure(configure ?? (static _ => { }));
    services.TryAddSingleton<CorrelationHeaderMiddleware>();

    return services;
  }

  /// <summary>
  /// Registers a background service that runs periodically.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to register the service to.</param>
  /// <param name="configure">An <see cref="System.Action{T}" /> to configure the provided <see cref="PeriodicBackgroundServiceOptions" />.</param>
  /// <typeparam name="TService">The type of <see cref="PeriodicBackgroundService{TService,TOptions}"/> to register.</typeparam>
  /// <typeparam name="TOptions">The type of <see cref="PeriodicBackgroundServiceOptions"/> to use in configuring the <see cref="PeriodicBackgroundService{TService,TOptions}"/>.</typeparam>
  /// <returns>A <see cref="IServiceCollection"/> containing <see cref="PeriodicBackgroundService{TService,TOptions}"/>.</returns>
  public static IServiceCollection AddPeriodicBackgroundService<TService, TOptions>(this IServiceCollection services,
    Action<TOptions>? configure = null)
    where TService : PeriodicBackgroundService<TService, TOptions>
    where TOptions : PeriodicBackgroundServiceOptions, IOptionsDefinition, new()
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOptionsDefinition<TOptions>();
    services.Configure(configure ?? (static _ => { }));
    return services.AddHostedService<TService>();
  }

  /// <summary>
  /// Registers all implementations of <see cref="IEndpoint"/> in the provided assembly.
  /// </summary>
  /// <param name="services"><see cref="IServiceCollection"/> to configure.</param>
  /// <param name="assemblyType">A type in the assembly to scan for <see cref="IEndpoint"/> implementations.</param>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="assemblyType"/> are null.</exception>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  public static IServiceCollection AddEndpoints(this IServiceCollection services, Type assemblyType)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(assemblyType);

    return services.AddEndpoints(assemblyType.Assembly);
  }

  /// <summary>
  /// Registers all implementations of <see cref="IEndpoint"/> in the provided assembly.
  /// </summary>
  /// <param name="services"><see cref="IServiceCollection"/> to configure.</param>
  /// <param name="assembly">The assembly to scan for <see cref="IEndpoint"/> implementations.</param>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="assembly"/> are null.</exception>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(assembly);

    var serviceDescriptors = assembly
      .DefinedTypes
      .Where(static type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)))
      .Select(static type => ServiceDescriptor.Singleton(typeof(IEndpoint), type))
      .ToArray();

    services.TryAddEnumerable(serviceDescriptors);
    return services;
  }
}
