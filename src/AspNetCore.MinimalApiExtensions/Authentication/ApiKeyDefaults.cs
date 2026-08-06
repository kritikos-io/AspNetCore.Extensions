namespace Kritikos.AspNetCore.MinimalApiExtensions.Authentication;

/// <summary>
/// Default values for API-key authentication.
/// </summary>
public static class ApiKeyDefaults
{
  /// <summary>The default API-key authentication scheme name.</summary>
  public const string AuthenticationScheme = "ApiKey";

  /// <summary>The default request header carrying the API key.</summary>
  public const string HeaderName = "X-API-Key";
}
