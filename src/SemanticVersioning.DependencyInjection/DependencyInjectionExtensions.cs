namespace Kritikos.SemanticVersioning.DependencyInjection;

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Dependency injection extensions for registering <see cref="SemanticVersionDescriptor"/> instances.
/// </summary>
public static class DependencyInjectionExtensions
{
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  extension(IServiceCollection services)
  {
    /// <summary>
    /// Registers a <see cref="SemanticVersionDescriptor"/> that is created from the <see cref="AssemblyInformationalVersionAttribute"/> of the provided assembly.
    /// </summary>
    /// <param name="assembly">The assembly that will provide the version number.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentException">The assembly has no <see cref="AssemblyInformationalVersionAttribute"/>, or its value is not in a valid semantic versioning format.</exception>
    /// <remarks>Registered via <c>TryAddSingleton</c>, so an existing registration is left untouched.</remarks>
    public IServiceCollection AddSemanticVersionDescriptor(Assembly assembly)
    {
      ArgumentNullException.ThrowIfNull(services);
      services.TryAddSingleton(SemanticVersionDescriptor.FromAssembly(assembly));

      return services;
    }

    /// <summary>
    /// Registers a <see cref="SemanticVersionDescriptor"/> that is created from the <see cref="AssemblyInformationalVersionAttribute"/> of the assembly containing the provided type.
    /// </summary>
    /// <param name="type">A type contained in the assembly that should provide the version number.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The assembly has no <see cref="AssemblyInformationalVersionAttribute"/>, or its value is not in a valid semantic versioning format.</exception>
    /// <remarks>Registered via <c>TryAddSingleton</c>, so an existing registration is left untouched.</remarks>
    public IServiceCollection AddSemanticVersionDescriptor(Type type)
    {
      ArgumentNullException.ThrowIfNull(services);
      services.TryAddSingleton(SemanticVersionDescriptor.FromType(type));

      return services;
    }
  }
}
