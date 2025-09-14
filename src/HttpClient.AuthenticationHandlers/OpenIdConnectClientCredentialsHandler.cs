namespace Kritikos.HttpClient.AuthenticationHandlers;

using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication.JwtBearer;

public class OpenIdConnectClientCredentialsHandler(OpenIdConnectTokenProvider tokenProvider) : DelegatingHandler
{
  private readonly OpenIdConnectTokenProvider tokenProvider = tokenProvider;

  /// <inheritdoc />
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    var authToken = await tokenProvider.GetAccessToken(cancellationToken);
    request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, authToken);

    return await base.SendAsync(request, cancellationToken);
  }
}
