namespace Kritikos.HttpClient.AuthenticationHandlers;

public class OpenIdConnectHandlerOptions
{
  public Uri WellKnownEndpoint { get; set; } = new Uri("about:blank");

  public string ClientId { get; set; }

  public string ClientSecret { get; set; }
}
