namespace Kritikos.AspNetCore.OpenApiExtensions.Tests;

using Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Microsoft.AspNetCore.OpenApi;

using Microsoft.OpenApi;

using NSubstitute;

public class ApiKeySecuritySchemeDocumentTransformerTests
{
  [Test]
  public async Task Constructor_whitespace_header_throws()
    => await Assert.That(() => new ApiKeySecuritySchemeDocumentTransformer("  "))
      .Throws<ArgumentException>();

  [Test]
  public async Task TransformAsync_null_document_throws()
  {
    var transformer = new ApiKeySecuritySchemeDocumentTransformer("X-API-Key");

    await Assert.That(async () =>
        await transformer.TransformAsync(null!, CreateContext(), CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task Adds_api_key_header_scheme()
  {
    var transformer = new ApiKeySecuritySchemeDocumentTransformer("X-API-Key");
    var document = new OpenApiDocument();

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    await Assert.That(document.Components!.SecuritySchemes!.ContainsKey("apiKey")).IsTrue();
    var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["apiKey"];
    await Assert.That(scheme.Type == SecuritySchemeType.ApiKey).IsTrue();
    await Assert.That(scheme.In == ParameterLocation.Header).IsTrue();
    await Assert.That(scheme.Name).IsEqualTo("X-API-Key");
  }

  [Test]
  public async Task Existing_scheme_is_not_overwritten()
  {
    var transformer = new ApiKeySecuritySchemeDocumentTransformer("X-API-Key");
    var document = new OpenApiDocument
    {
      Components = new OpenApiComponents
      {
        SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal)
        {
          ["apiKey"] = new OpenApiSecurityScheme { Description = "preset" },
        },
      },
    };

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    var scheme = (OpenApiSecurityScheme)document.Components.SecuritySchemes["apiKey"];
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
