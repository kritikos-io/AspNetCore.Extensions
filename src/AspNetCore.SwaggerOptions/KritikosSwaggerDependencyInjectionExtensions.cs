// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.SwaggerOptions;

using Swashbuckle.AspNetCore.SwaggerGen;

public static class KritikosSwaggerDependencyInjectionExtensions
{
  /// <summary>
  /// Adds opinionated default options for swagger.
  /// </summary>
  /// <param name="services"><see cref="IServiceCollection"/> to configure.</param>
  /// <exception cref="ArgumentNullException"><paramref name="services"/> are null.</exception>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  public static IServiceCollection AddSwaggerDefaults(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.ConfigureOptions<SwaggerDefaultOptions>();

    return services;
  }

  /// <summary>
  /// Adds xmldoc from controllers as OpenApi documentation.
  /// </summary>
  /// <param name="options">The <see cref="SwaggerGenOptions"/> to configure.</param>
  /// <param name="containingAssembly"><see cref="Type"/> contained in the assembly to pick the documentation from.</param>
  /// <exception cref="ArgumentNullException"><paramref name="options"/> or <paramref name="containingAssembly"/> are null.</exception>
  public static void AddXmlDocumentation(this SwaggerGenOptions options, Type containingAssembly)
  {
    ArgumentNullException.ThrowIfNull(options);
    ArgumentNullException.ThrowIfNull(containingAssembly);

    var xmlFile = $"{containingAssembly.Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
  }
}
