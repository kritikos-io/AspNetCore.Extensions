namespace Kritikos.AspNetCore.MinimalApiExtensions.Authentication;

using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Extensions for registering the API-key authentication scheme. Requires an <see cref="IApiKeyValidator"/> to be
/// registered in the service collection.
/// </summary>
public static class ApiKeyAuthenticationExtensions
{
  /// <param name="builder">The <see cref="AuthenticationBuilder"/> to add the scheme to.</param>
  extension(AuthenticationBuilder builder)
  {
    /// <summary>
    /// Adds the API-key authentication scheme under <see cref="ApiKeyDefaults.AuthenticationScheme"/>.
    /// </summary>
    /// <param name="configureOptions">An optional action to configure the scheme options.</param>
    /// <returns>The <see cref="AuthenticationBuilder"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public AuthenticationBuilder AddApiKey(Action<ApiKeyAuthenticationOptions>? configureOptions = null)
    {
      ArgumentNullException.ThrowIfNull(builder);

      return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyDefaults.AuthenticationScheme, configureOptions);
    }

    /// <summary>
    /// Adds the API-key authentication scheme under <paramref name="authenticationScheme"/>.
    /// </summary>
    /// <param name="authenticationScheme">The scheme name to register.</param>
    /// <param name="configureOptions">An optional action to configure the scheme options.</param>
    /// <returns>The <see cref="AuthenticationBuilder"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public AuthenticationBuilder AddApiKey(
      string authenticationScheme,
      Action<ApiKeyAuthenticationOptions>? configureOptions)
    {
      ArgumentNullException.ThrowIfNull(builder);

      return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        authenticationScheme, configureOptions);
    }
  }
}
