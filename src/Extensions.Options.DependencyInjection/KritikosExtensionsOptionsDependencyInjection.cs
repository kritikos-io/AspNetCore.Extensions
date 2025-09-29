namespace Kritikos.Extensions.Options.DependencyInjection;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kritikos.Extensions.Options.Contracts;

using Microsoft.Extensions.DependencyInjection;

public static class KritikosExtensionsOptionsDependencyInjection
{
  private static readonly ConcurrentDictionary<Type, Action<IServiceCollection>?> CachedInvokers = new();

  private static readonly MethodInfo? AddOptionsDefinitionMethodInfo =
    typeof(KritikosExtensionsOptionsDependencyInjection)
      .GetMethod(nameof(AddOptionsDefinition), BindingFlags.Static | BindingFlags.Public);

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

  /// <summary>
  /// Adds services required for using options and enforces options validation check on start rather than in runtime.
  /// </summary>
  /// <remarks>
  /// The <seealso cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}(Microsoft.Extensions.Options.OptionsBuilder{TOptions})"/> extension is called by this method.
  /// </remarks>
  /// <typeparam name="TOptions">The options type to be configured.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="name">The name of the options instance.</param>
  /// <param name="location">The name of the configuration section to bind from for named instances.</param>
  /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
  public static IServiceCollection AddOptionsDefinition<TOptions>(this IServiceCollection services)
    where TOptions : class, IOptionsDefinition
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOptionsWithValidateOnStart<TOptions>()
      .BindConfiguration(TOptions.Location)
      .ValidateDataAnnotations();

    return services;
  }

  /// <summary>
  /// Adds all discovered <seealso cref="IOptionsDefinition"/> implementations from the provided assembly to the <see cref="IServiceCollection"/>.
  /// </summary>
  /// <remarks>
  /// The <seealso cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}(Microsoft.Extensions.Options.OptionsBuilder{TOptions})"/> extension is called by this method.
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
  /// The <seealso cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}(Microsoft.Extensions.Options.OptionsBuilder{TOptions})"/> extension is called by this method.
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
}
