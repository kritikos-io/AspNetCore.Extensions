namespace Kritikos.HttpClient.Handlers;

using Kritikos.HttpClient.Handlers.Contracts;

/// <summary>
/// A simple handler that adds a random user agent to the request if one does not already exist.
/// </summary>
/// <param name="provider">An implementation of <see cref="IUserAgentProvider"/> to handle the generation of user agent strings.</param>
/// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
public class UserAgentHandler(IUserAgentProvider provider, HttpMessageHandler innerHandler)
    : DelegatingHandler(innerHandler)
{
  /// <inheritdoc />
  protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    if (!request.Headers.Contains("User-Agent"))
    {
      request.Headers.TryAddWithoutValidation("User-Agent", provider.GetRandomUserAgent());
    }

    return await base.SendAsync(request, cancellationToken);
  }
}
