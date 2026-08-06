namespace Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using Kritikos.Extensions.Options.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for the RFC 8615 well-known endpoints.
/// </summary>
public static class KritikosWellKnownServiceCollectionExtensions
{
  /// <param name="services">The <see cref="IServiceCollection"/> to register the options onto.</param>
  extension(IServiceCollection services)
  {
    /// <summary>
    /// Binds and validates <see cref="SecurityTxtOptions"/> for the <c>/.well-known/security.txt</c> endpoint.
    /// </summary>
    /// <param name="configure">An optional action to configure the options after binding.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public IServiceCollection AddSecurityTxt(Action<SecurityTxtOptions>? configure = null)
    {
      ArgumentNullException.ThrowIfNull(services);

      services.AddOptionsDefinition<SecurityTxtOptions>();
      services.Configure(configure ?? (static _ => { }));

      return services;
    }

    /// <summary>
    /// Binds and validates <see cref="OAuthProtectedResourceOptions"/> for the
    /// <c>/.well-known/oauth-protected-resource</c> endpoint.
    /// </summary>
    /// <param name="configure">An optional action to configure the options after binding.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public IServiceCollection AddOAuthProtectedResource(Action<OAuthProtectedResourceOptions>? configure = null)
    {
      ArgumentNullException.ThrowIfNull(services);

      services.AddOptionsDefinition<OAuthProtectedResourceOptions>();
      services.Configure(configure ?? (static _ => { }));

      return services;
    }
  }
}
