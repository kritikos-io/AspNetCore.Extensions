namespace Kritikos.AspNetCore.OpenApiExtensions.Tests;

using Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Microsoft.AspNetCore.OpenApi;

using Microsoft.OpenApi;

using NSubstitute;

public class BearerSecuritySchemeDocumentTransformerTests
{
  [Test]
  public async Task TransformAsync_null_document_throws()
  {
    var transformer = new BearerSecuritySchemeDocumentTransformer();

    await Assert.That(async () =>
        await transformer.TransformAsync(null!, CreateContext(), CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task Adds_http_bearer_jwt_scheme()
  {
    var transformer = new BearerSecuritySchemeDocumentTransformer();
    var document = new OpenApiDocument();

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    await Assert.That(document.Components!.SecuritySchemes!.ContainsKey("bearer")).IsTrue();
    var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["bearer"];
    await Assert.That(scheme.Type == SecuritySchemeType.Http).IsTrue();
    await Assert.That(scheme.Scheme).IsEqualTo("bearer");
    await Assert.That(scheme.BearerFormat).IsEqualTo("JWT");
  }

  [Test]
  public async Task Existing_scheme_is_not_overwritten()
  {
    var transformer = new BearerSecuritySchemeDocumentTransformer();
    var document = new OpenApiDocument
    {
      Components = new OpenApiComponents
      {
        SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal)
        {
          ["bearer"] = new OpenApiSecurityScheme { Description = "preset" },
        },
      },
    };

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["bearer"];
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
