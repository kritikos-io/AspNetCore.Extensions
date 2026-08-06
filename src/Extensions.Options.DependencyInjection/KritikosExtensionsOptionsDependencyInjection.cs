namespace Kritikos.Extensions.Options.DependencyInjection;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kritikos.Extensions.Options.Contracts;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for registering <see cref="IOptionsDefinition"/> implementations.
/// </summary>
public static class KritikosExtensionsOptionsDependencyInjection
{
  private static readonly ConcurrentDictionary<Type, Action<IServiceCollection>?> CachedInvokers = new();

  private static readonly MethodInfo? AddOptionsDefinitionMethodInfo =
    typeof(KritikosExtensionsOptionsDependencyInjection)
      .GetMethod(nameof(AddOptionsDefinition), BindingFlags.Static | BindingFlags.Public);

  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  extension(IServiceCollection services)
  {
    /// <summary>
    /// Adds services required for using options and enforces options validation check on start rather than in runtime.
    /// </summary>
    /// <remarks>
    /// The <seealso cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}(Microsoft.Extensions.Options.OptionsBuilder{TOptions})"/> extension is called by this method.
    /// </remarks>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">The <see cref="IServiceCollection"/> is <see langword="null"/>.</exception>
    public IServiceCollection AddOptionsDefinition<TOptions>()
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
    /// <param name="type">The type used to discover the assembly containing <seealso cref="IOptionsDefinition"/> implementations.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/>, or the <see cref="IServiceCollection"/>, is <see langword="null"/>.</exception>
    public IServiceCollection AddOptionsDefinitions(Type type)
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
    /// <param name="assembly">The assembly to scan for <seealso cref="IOptionsDefinition"/> implementations.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
    public IServiceCollection AddOptionsDefinitions(Assembly assembly)
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
