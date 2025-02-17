namespace Kritikos.AspNetCore.OpenApiOidcExtensions.DocumentTransformers;

using Kritikos.AspNetCore.OpenApiOidcExtensions.Options;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;

public sealed class OidcSecuritySchemeTransformer<TOpenIdOptions>(IOptions<TOpenIdOptions> options, IHttpClientFactory clientFactory)
    : IOpenApiDocumentTransformer
    where TOpenIdOptions : OpenApiOpenIdOptions
{
  private readonly IHttpClientFactory clientFactory = clientFactory;
  private readonly TOpenIdOptions options = options.Value;

  public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
  {
    document.Components ??= new OpenApiComponents();
    using var client = clientFactory.CreateClient();
    var response = await client.GetStringAsync(new Uri($"{options.Authority}/.well-known/openid-configuration"), cancellationToken);
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
          Scopes = { { "openid", "Basic authentication" }, { "profile", "Access your profile" }, { "email", "Access your email" }, },
          AuthorizationUrl = new Uri(oidc.AuthorizationEndpoint),
          TokenUrl = new Uri(oidc.TokenEndpoint),
        },
      },
    };

    document.Components.SecuritySchemes.TryAdd("openid", oidcScheme);

    document.SecurityRequirements.Add(new OpenApiSecurityRequirement() { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "openid" } }] = ["openid", "profile", "email"] });
  }
}
