namespace Kritikos.AspNetCore.MinimalApiExtensions.Authentication;

using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Authenticates requests by reading an API key from a configurable header and validating it through
/// <see cref="IApiKeyValidator"/>. Requests without the header are left unauthenticated so other schemes may run.
/// </summary>
/// <param name="options">The monitor for the scheme's options.</param>
/// <param name="logger">The logger factory.</param>
/// <param name="encoder">The URL encoder.</param>
/// <param name="validator">The validator resolving keys to principals.</param>
public sealed class ApiKeyAuthenticationHandler(
  IOptionsMonitor<ApiKeyAuthenticationOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder,
  IApiKeyValidator validator)
  : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
  /// <inheritdoc />
  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    if (!Request.Headers.TryGetValue(Options.HeaderName, out var values))
    {
      return AuthenticateResult.NoResult();
    }

    var apiKey = values.ToString();
    if (string.IsNullOrEmpty(apiKey))
    {
      return AuthenticateResult.NoResult();
    }

    var principal = await validator.ValidateAsync(apiKey, Context.RequestAborted).ConfigureAwait(false);

    return principal is null
      ? AuthenticateResult.Fail("Invalid API key.")
      : AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
  }
}
