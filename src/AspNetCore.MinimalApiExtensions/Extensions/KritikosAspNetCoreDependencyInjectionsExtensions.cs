namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;
using Kritikos.AspNetCore.MinimalApiExtensions.Middleware;
using Kritikos.AspNetCore.MinimalApiExtensions.Options;
using Kritikos.AspNetCore.MinimalApiExtensions.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

public static class KritikosAspNetCoreDependencyInjectionsExtensions
{
  private static readonly ConcurrentDictionary<Type, Action<IServiceCollection>?> CachedInvokers = new();

  private static readonly MethodInfo? AddOptionsDefinitionMethodInfo =
    typeof(KritikosAspNetCoreDependencyInjectionsExtensions)
      .GetMethod(nameof(AddOptionsDefinition), BindingFlags.Static | BindingFlags.Public);

  private static Action<IServiceCollection>? BuildOptionsDefinitionInvoker(Type optionsType)
  {
    var closedMethod = AddOptionsDefinitionMethodInfo?.MakeGenericMethod(optionsType);
    if (closedMethod is null)
    {
      return null;
    }

    var servicesParameter = Expression.Parameter(typeof(IServiceCollection), "services");
    var name = Expression.Constant(null, typeof(string));
    var location = Expression.Constant(null, typeof(string));
    var call = Expression.Call(closedMethod, servicesParameter, name, location);
    return Expression.Lambda<Action<IServiceCollection>>(call, servicesParameter).Compile();
  }

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
  /// Adds services required for using options and enforces options validation check on start rather than in runtime.
  /// </summary>
  /// <remarks>
  /// The <seealso cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}(OptionsBuilder{TOptions})"/> extension is called by this method.
  /// </remarks>
  /// <typeparam name="TOptions">The options type to be configured.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="name">The name of the options instance.</param>
  /// <param name="location">The name of the configuration section to bind from for named instances.</param>
  /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
  public static IServiceCollection AddOptionsDefinition<TOptions>(this IServiceCollection services, string? name = null,
    string? location = null)
    where TOptions : class, IOptionsDefinition
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOptionsWithValidateOnStart<TOptions>(name)
      .BindConfiguration(
        string.IsNullOrWhiteSpace(name)
          ? TOptions.Location
          : location ?? Options.DefaultName)
      .ValidateDataAnnotations();

    return services;
  }

  /// <summary>
  /// Adds all discovered <seealso cref="IOptionsDefinition"/> implementations from the provided assembly to the <see cref="IServiceCollection"/>.
  /// </summary>
  /// <remarks>
  /// The <seealso cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}(OptionsBuilder{TOptions})"/> extension is called by this method.
  /// </remarks>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="type">The type used to discover the assembly containing <seealso cref="IOptionsDefinition"/> implementations.</param>
  /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
  public static IServiceCollection AddOptionsDefinitions(this IServiceCollection services, Type type)
  {
    ArgumentNullException.ThrowIfNull(type);
    ArgumentNullException.ThrowIfNull(services);

    return services.AddOptionsDefinitions(type.Assembly);
  }

  /// <summary>
  /// Adds all discovered <seealso cref="IOptionsDefinition"/> implementations from the provided assembly to the <see cref="IServiceCollection"/>.
  /// </summary>
  /// <remarks>
  /// The <seealso cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}(OptionsBuilder{TOptions})"/> extension is called by this method.
  /// </remarks>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="assembly">The assembly to scan for <seealso cref="IOptionsDefinition"/> implementations.</param>
  /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
  public static IServiceCollection AddOptionsDefinitions(this IServiceCollection services, Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(assembly);

    foreach (var typeInfo in assembly.DefinedTypes)
    {
      if (typeInfo.IsAbstract || typeInfo.IsInterface || !typeInfo.IsAssignableTo(typeof(IOptionsDefinition)))
      {
        continue;
      }

      CachedInvokers.GetOrAdd(typeInfo.AsType(), BuildOptionsDefinitionInvoker)
        ?.Invoke(services);
    }

    return services;
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
