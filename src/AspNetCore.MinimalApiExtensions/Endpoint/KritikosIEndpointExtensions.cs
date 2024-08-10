// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.Extensions.DependencyInjection.Extensions;

public static partial class KritikosIEndpointExtensions
{
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
        .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)))
        .Select(type => ServiceDescriptor.Singleton(typeof(IEndpoint), type))
        .ToArray();

    services.TryAddEnumerable(serviceDescriptors);
    return services;
  }

  /// <summary>
  /// Maps all endpoints defined in implementations of <see cref="IEndpoint"/>.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  /// <remarks>Only <see cref="IEndpoint"/> implementations registered by <see cref="AddEndpoints(IServiceCollection,Assembly)"/> or <see cref="AddEndpoints(IServiceCollection,Type)"/> are mapped.</remarks>
  public static WebApplication MapEndpoints(this WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);

    var endpoints = app.Services.GetService<IEnumerable<IEndpoint>>() ?? [];
    foreach (var endpoint in endpoints)
    {
      endpoint.MapEndpoint(app);
    }

    return app;
  }
}
