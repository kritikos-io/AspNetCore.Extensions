namespace Kritikos.HttpClient.AuthenticationHandlers;

using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication.JwtBearer;

/// <summary>
/// A delegating handler that attaches an OAuth 2.0 client credentials access token to outgoing HTTP requests.
/// </summary>
/// <param name="tokenProvider">The provider used to obtain access tokens.</param>
public class OpenIdConnectClientCredentialsHandler(OpenIdConnectTokenProvider tokenProvider) : DelegatingHandler
{
  private readonly OpenIdConnectTokenProvider tokenProvider = tokenProvider;

  /// <inheritdoc />
  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    var authToken = await tokenProvider.GetAccessToken(cancellationToken);
    request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authToken);

    return await base.SendAsync(request, cancellationToken);
  }
}
