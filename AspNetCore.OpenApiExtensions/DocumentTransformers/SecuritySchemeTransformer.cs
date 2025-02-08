using Microsoft.OpenApi.Models;

namespace Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Kritikos.AspNetCore.OpenApiExtensions.Options;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

public sealed class SecuritySchemeTransformer(IOptions<OpenIdOptions> options)
    : IOpenApiDocumentTransformer
{
  private readonly OpenIdOptions options = options.Value;

  public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
  {
    document.Components ??= new OpenApiComponents();

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
          AuthorizationUrl = new Uri($"{options.Authority}/protocol/openid-connect/auth"),
          TokenUrl = new Uri($"{options.Authority}/protocol/openid-connect/token"),
        },
      },
    };

    document.Components.SecuritySchemes.TryAdd("openid", oidcScheme);

    document.SecurityRequirements.Add(new OpenApiSecurityRequirement() { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "openid" } }] = new[] { "openid", "profile", "email" } });
  }
}
