namespace Kritikos.AspNetCore.OpenApiExtensions.Tests;

using Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Microsoft.AspNetCore.OpenApi;

using Microsoft.OpenApi;

using NSubstitute;

public class OAuth2SecuritySchemeDocumentTransformerTests
{
  private static readonly Uri AuthorizationUrl = new("https://auth.example/protocol/openid-connect/auth");
  private static readonly Uri TokenUrl = new("https://auth.example/protocol/openid-connect/token");

  [Test]
  public async Task Constructor_null_authorization_url_throws()
    => await Assert.That(() => new OAuth2SecuritySchemeDocumentTransformer(null!, TokenUrl))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task Constructor_null_token_url_throws()
    => await Assert.That(() => new OAuth2SecuritySchemeDocumentTransformer(AuthorizationUrl, null!))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task TransformAsync_null_document_throws()
  {
    var transformer = new OAuth2SecuritySchemeDocumentTransformer(AuthorizationUrl, TokenUrl);

    await Assert.That(async () =>
        await transformer.TransformAsync(null!, CreateContext(), CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task Adds_oauth2_authorization_code_scheme()
  {
    var transformer = new OAuth2SecuritySchemeDocumentTransformer(
      AuthorizationUrl,
      TokenUrl,
      new Dictionary<string, string>(StringComparer.Ordinal) { ["openid"] = "Basic authentication" });
    var document = new OpenApiDocument();

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    await Assert.That(document.Components!.SecuritySchemes!.ContainsKey("oauth2")).IsTrue();
    var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["oauth2"];
    await Assert.That(scheme.Type == SecuritySchemeType.OAuth2).IsTrue();

    var flow = scheme.Flows!.AuthorizationCode!;
    await Assert.That(flow.AuthorizationUrl).IsEqualTo(AuthorizationUrl);
    await Assert.That(flow.TokenUrl).IsEqualTo(TokenUrl);
    await Assert.That(flow.Scopes!.ContainsKey("openid")).IsTrue();
  }

  [Test]
  public async Task Existing_scheme_is_not_overwritten()
  {
    var transformer = new OAuth2SecuritySchemeDocumentTransformer(AuthorizationUrl, TokenUrl);
    var document = new OpenApiDocument
    {
      Components = new OpenApiComponents
      {
        SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal)
        {
          ["oauth2"] = new OpenApiSecurityScheme { Description = "preset" },
        },
      },
    };

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["oauth2"];
    await Assert.That(scheme.Description).IsEqualTo("preset");
  }

  private static OpenApiDocumentTransformerContext CreateContext()
    => new()
    {
      DocumentName = "v1",
      DescriptionGroups = [],
      ApplicationServices = Substitute.For<IServiceProvider>(),
    };
}
