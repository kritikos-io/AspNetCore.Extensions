namespace Kritikos.AspNetCore.OpenApiVersioningOptions.Tests;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using Kritikos.AspNetCore.OpenApiVersioningOptions.DocumentTransformers;
using Kritikos.AspNetCore.OpenApiVersioningOptions.Options;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

using NSubstitute;

public class ApiVersionDocumentTransformerTests
{
  private const string DocumentName = "v1";

  [Test]
  public async Task TransformAsync_null_document_throws()
  {
    var transformer = CreateTransformer(CreateDescription());

    await Assert.That(async () =>
        await transformer.TransformAsync(null!, CreateContext(DocumentName), CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task Matching_document_name_populates_info()
  {
    var transformer = CreateTransformer(CreateDescription(groupName: DocumentName));
    var document = new OpenApiDocument { Info = new OpenApiInfo() };

    await transformer.TransformAsync(document, CreateContext(DocumentName), CancellationToken.None);

    await Assert.That(document.Info.Title).IsEqualTo("Pet Store");
    await Assert.That(document.Info.Version).IsEqualTo("1.0");
    await Assert.That(document.Info.Description).IsEqualTo("A sample API.");
    await Assert.That(document.Info.Contact!.Name).IsEqualTo("Support");
    await Assert.That(document.Info.Contact.Email).IsEqualTo("support@example.test");
    await Assert.That(document.Info.License!.Name).IsEqualTo("MIT");
  }

  [Test]
  public async Task Non_matching_document_name_leaves_info_untouched()
  {
    var transformer = CreateTransformer(CreateDescription(groupName: "v2"));
    var document = new OpenApiDocument { Info = new OpenApiInfo { Title = "unchanged" } };

    await transformer.TransformAsync(document, CreateContext(DocumentName), CancellationToken.None);

    await Assert.That(document.Info.Title).IsEqualTo("unchanged");
  }

  [Test]
  public async Task Deprecated_version_is_noted_in_description()
  {
    var transformer = CreateTransformer(CreateDescription(groupName: DocumentName, deprecated: true));
    var document = new OpenApiDocument { Info = new OpenApiInfo() };

    await transformer.TransformAsync(document, CreateContext(DocumentName), CancellationToken.None);

    await Assert.That(document.Info.Description).Contains("This API version has been deprecated.");
  }

  [Test]
  public async Task Sunset_policy_is_rendered_in_description()
  {
    var link = new LinkHeaderValue(new Uri("https://example.test/migrate"), "sunset")
    {
      Type = "text/html",
      Title = "Migration guide",
    };
    var sunset = new SunsetPolicy(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), link);
    var transformer = CreateTransformer(CreateDescription(groupName: DocumentName, sunset: sunset));
    var document = new OpenApiDocument { Info = new OpenApiInfo() };

    await transformer.TransformAsync(document, CreateContext(DocumentName), CancellationToken.None);

    await Assert.That(document.Info.Description).Contains("This API version will be sunset on");
    await Assert.That(document.Info.Description).Contains("Migration guide");
  }

  // ---- Helpers ----
  private static ApiVersionDocumentTransformer<TestApiInfoOptions> CreateTransformer(
    params ApiVersionDescription[] descriptions)
  {
    var versionProvider = Substitute.For<IApiVersionDescriptionProvider>();
    versionProvider.ApiVersionDescriptions.Returns(descriptions);
    var options = Options.Create(new TestApiInfoOptions
    {
      Title = "Pet Store",
      Description = "A sample API.",
      ContactName = "Support",
      ContactEmail = "support@example.test",
      LicenseName = "MIT",
      LicenseUrl = new Uri("https://example.test/license"),
    });

    return new ApiVersionDocumentTransformer<TestApiInfoOptions>(options, versionProvider);
  }

  private static ApiVersionDescription CreateDescription(
    string groupName = DocumentName,
    bool deprecated = false,
    SunsetPolicy? sunset = null)
    => new(new ApiVersion(1, 0, null), groupName, deprecated, sunset, null);

  private static OpenApiDocumentTransformerContext CreateContext(string documentName)
    => new()
    {
      DocumentName = documentName,
      DescriptionGroups = [],
      ApplicationServices = Substitute.For<IServiceProvider>(),
    };

  private sealed class TestApiInfoOptions : OpenApiInfoOptions
  {
  }
}
