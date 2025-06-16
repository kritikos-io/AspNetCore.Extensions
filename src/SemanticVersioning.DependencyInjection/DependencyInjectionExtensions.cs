namespace Kritikos.SemanticVersioning.DependencyInjection;

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddSemanticVersionDescriptor(this IServiceCollection services, Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(services);
    services.AddSingleton(SemanticVersionDescriptor.FromAssembly(assembly));

    return services;
  }

  public static IServiceCollection AddSemanticVersionDescriptor(this IServiceCollection services, Type type)
  {
    ArgumentNullException.ThrowIfNull(services);
    services.AddSingleton(SemanticVersionDescriptor.FromType(type));

    return services;
  }
}
