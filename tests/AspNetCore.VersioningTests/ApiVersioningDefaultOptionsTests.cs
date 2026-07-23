namespace Kritikos.AspNetCore.VersioningTests;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using Kritikos.AspNetCore.VersioningOptions;

using Microsoft.AspNetCore.Http;

public class ApiVersioningDefaultOptionsTests
{
  [Test]
  public async Task Configure_api_versioning_options_applies_defaults()
  {
    var options = new ApiVersioningOptions();

    new ApiVersioningDefaultOptions().Configure(options);

    await Assert.That(options.DefaultApiVersion).IsEqualTo(new ApiVersion(1));
    await Assert.That(options.ReportApiVersions).IsTrue();
    await Assert.That(options.AssumeDefaultVersionWhenUnspecified).IsTrue();
    await Assert.That(options.ApiVersionReader is UrlSegmentApiVersionReader).IsTrue();
    await Assert.That(options.UnsupportedApiVersionStatusCode).IsEqualTo(StatusCodes.Status503ServiceUnavailable);
  }

  [Test]
  public async Task Configure_api_explorer_options_applies_defaults()
  {
    var options = new ApiExplorerOptions();

    new ApiVersioningDefaultOptions().Configure(options);

    await Assert.That(options.AssumeDefaultVersionWhenUnspecified).IsTrue();
    await Assert.That(options.SubstituteApiVersionInUrl).IsTrue();
    await Assert.That(options.RouteConstraintName).IsEqualTo("apiVersion");
    await Assert.That(options.GroupNameFormat).IsEqualTo("'v'V");
  }

  [Test]
  public async Task Configure_null_api_versioning_options_throws()
    => await Assert.That(() => new ApiVersioningDefaultOptions().Configure((ApiVersioningOptions)null!))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task Configure_null_api_explorer_options_throws()
    => await Assert.That(() => new ApiVersioningDefaultOptions().Configure((ApiExplorerOptions)null!))
      .Throws<ArgumentNullException>();
}
