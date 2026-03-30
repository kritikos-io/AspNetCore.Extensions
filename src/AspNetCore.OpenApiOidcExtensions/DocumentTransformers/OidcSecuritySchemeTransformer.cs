namespace Kritikos.AspNetCore.OpenApiOidcExtensions.DocumentTransformers;

using Kritikos.AspNetCore.OpenApiOidcExtensions.Options;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi;

/// <summary>
/// An OpenAPI document transformer that adds an OpenID Connect OAuth2 security scheme by fetching the OIDC discovery document.
/// </summary>
/// <typeparam name="TOpenIdOptions">The options type containing the OIDC authority configuration.</typeparam>
/// <param name="options">The OpenID Connect options providing the authority URL.</param>
/// <param name="clientFactory">The HTTP client factory used to fetch the OIDC discovery document.</param>
public sealed class OidcSecuritySchemeTransformer<TOpenIdOptions>(
  IOptions<TOpenIdOptions> options,
  IHttpClientFactory clientFactory)
  : IOpenApiDocumentTransformer
  where TOpenIdOptions : OpenApiOpenIdOptions
{
  private readonly IHttpClientFactory clientFactory = clientFactory;
  private readonly TOpenIdOptions options = options.Value;

  /// <inheritdoc />
  public async Task TransformAsync(
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(document);

    document.Components ??= new OpenApiComponents();
    using var client = clientFactory.CreateClient();
    var response = await client.GetStringAsync(
      new Uri($"{options.Authority}/.well-known/openid-configuration"),
      cancellationToken);
    var oidc = OpenIdConnectConfiguration.Create(response);

    var oidcScheme = new OpenApiSecurityScheme()
    {
      Type = SecuritySchemeType.OAuth2,
      OpenIdConnectUrl = new Uri($"{options.Authority}/.well-known/openid-configuration"),
      Description = "Oauth2 authentication via OpenId Connect",
      In = ParameterLocation.Header,
      Flows = new()
      {
        AuthorizationCode = new OpenApiOAuthFlow
        {
          Scopes = new Dictionary<string, string>
          {
            { "openid", "Basic authentication" },
            { "profile", "Access your profile" },
            { "email", "Access your email" },
          },
          AuthorizationUrl = new Uri(oidc.AuthorizationEndpoint),
          TokenUrl = new Uri(oidc.TokenEndpoint),
        },
      },
    };

    document.Components.SecuritySchemes?.TryAdd("openid", oidcScheme);

    document.Security?.Add(new OpenApiSecurityRequirement()
    {
      [new OpenApiSecuritySchemeReference("openid", document)] =
        ["openid", "profile", "email"],
    });
  }
}
