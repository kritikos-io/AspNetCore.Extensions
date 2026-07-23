namespace Kritikos.Extensions.Options.DependencyInjection;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kritikos.Extensions.Options.Contracts;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for registering <see cref="INamedOptionsDefinition"/> implementations.
/// </summary>
public static class KritikosExtensionsNamedOptionsDependencyInjection
{
  private static readonly ConcurrentDictionary<Type, Action<IServiceCollection>?> CachedInvokers = new();

  private static readonly MethodInfo? AddOptionsDefinitionMethodInfo =
    typeof(KritikosExtensionsNamedOptionsDependencyInjection)
      .GetMethod(nameof(AddOptionsDefinition), BindingFlags.Static | BindingFlags.Public);

  /// <summary>
  /// Adds services required for using named options and enforces options validation check on start.
  /// </summary>
  /// <typeparam name="TOptions">The named options type to be configured.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
  public static IServiceCollection AddOptionsDefinition<TOptions>(this IServiceCollection services)
    where TOptions : class, INamedOptionsDefinition
  {
    ArgumentNullException.ThrowIfNull(services);

    foreach (var (name, location) in TOptions.NamedLocations)
    {
      services.AddOptionsWithValidateOnStart<TOptions>(name)
        .BindConfiguration(location)
        .ValidateDataAnnotations();
    }

    return services;
  }

  /// <summary>
  /// Scans the provided assembly and registers all <see cref="INamedOptionsDefinition"/> implementations.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="assembly">The assembly to scan for <see cref="INamedOptionsDefinition"/> implementations.</param>
  /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
  public static IServiceCollection AddNamedOptionsDefinitions(this IServiceCollection services, Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(assembly);

    foreach (var typeInfo in assembly.DefinedTypes)
    {
      if (typeInfo.IsAbstract || typeInfo.IsInterface || !typeInfo.IsAssignableTo(typeof(INamedOptionsDefinition)))
      {
        continue;
      }

      CachedInvokers.GetOrAdd(typeInfo.AsType(), BuildOptionsDefinitionInvoker)
        ?.Invoke(services);
    }

    return services;
  }

  private static Action<IServiceCollection>? BuildOptionsDefinitionInvoker(Type optionsType)
  {
    var closedMethod = AddOptionsDefinitionMethodInfo?.MakeGenericMethod(optionsType);
    if (closedMethod is null)
    {
      return null;
    }

    var servicesParameter = Expression.Parameter(typeof(IServiceCollection), "services");
    var call = Expression.Call(closedMethod, servicesParameter);
    return Expression.Lambda<Action<IServiceCollection>>(call, servicesParameter).Compile();
  }
}
