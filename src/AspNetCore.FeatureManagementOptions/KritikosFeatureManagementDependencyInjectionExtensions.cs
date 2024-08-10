// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;

public static class KritikosFeatureManagementDependencyInjectionExtensions
{
  /// <summary>
  /// Adds a Session Management for features. Ensure you have configured session state as per ASP.NET Core documentation.
  /// </summary>
  /// <param name="services"><see cref="IServiceCollection"/> to configure.</param>
  /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
  /// <exception cref="InvalidOperationException">When Session state has not been configured correctly (at runtime).</exception>
  /// <remarks>
  /// <![CDATA[https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state]]>
  /// </remarks>
  public static IServiceCollection AddFeatureManagementSessionManager(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.TryAddTransient<ISessionManager, SessionFeatureManager>();

    return services;
  }
}
