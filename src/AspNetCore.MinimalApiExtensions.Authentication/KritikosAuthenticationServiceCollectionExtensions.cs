namespace Kritikos.AspNetCore.MinimalApiExtensions.Authentication;

using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Dependency injection extensions that wire a JWT bearer scheme's authority into the RFC 9728
/// <c>/.well-known/oauth-protected-resource</c> document.
/// </summary>
public static class KritikosAuthenticationServiceCollectionExtensions
{
  /// <param name="services">The <see cref="IServiceCollection"/> to register onto.</param>
  extension(IServiceCollection services)
  {
    /// <summary>
    /// Registers the oauth-protected-resource options and adds the configured JWT bearer scheme's authority to its
    /// authorization servers, so the resource metadata follows the API's authentication configuration rather than a
    /// manually maintained list.
    /// </summary>
    /// <param name="configure">An optional action to configure the options (for example, supported scopes).</param>
    /// <param name="authenticationScheme">The JWT bearer scheme whose authority is advertised.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public IServiceCollection AddOidcProtectedResource(
      Action<OAuthProtectedResourceOptions>? configure = null,
      string authenticationScheme = JwtBearerDefaults.AuthenticationScheme)
    {
      ArgumentNullException.ThrowIfNull(services);

      services.AddOAuthProtectedResource(configure);
      services
        .AddOptions<OAuthProtectedResourceOptions>()
        .Configure<IOptionsMonitor<JwtBearerOptions>>((options, bearer) =>
        {
          if (Uri.TryCreate(bearer.Get(authenticationScheme).Authority, UriKind.Absolute, out var authority)
              && !options.AuthorizationServers.Contains(authority))
          {
            options.AuthorizationServers.Add(authority);
          }
        });

      return services;
    }
  }
}
