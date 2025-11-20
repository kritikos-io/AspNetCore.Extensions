namespace Kritikos.Extensions.Options.DependencyInjection;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kritikos.Extensions.Options.Contracts;

using Microsoft.Extensions.DependencyInjection;

public static class KritikosExtensionsNamedOptionsDependencyInjection
{
  private static readonly ConcurrentDictionary<Type, Action<IServiceCollection>?> CachedInvokers = new();

  private static readonly MethodInfo? AddOptionsDefinitionMethodInfo =
    typeof(KritikosExtensionsNamedOptionsDependencyInjection)
      .GetMethod(nameof(AddOptionsDefinition), BindingFlags.Static | BindingFlags.Public);

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
