namespace Kritikos.AspNetCore.OpenApiVersioningOptions.Tests;

using Asp.Versioning;

using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class OpenApiVersioningExtensionsTests
{
  [Test]
  public async Task AddVersionedOpenApi_null_services_throws()
    => await Assert.That(() => OpenApiVersioningExtensions.AddVersionedOpenApi<TestVersionModelProvider>(null!))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddVersionedOpenApi_configures_should_include_per_version()
  {
    var services = new ServiceCollection();

    services.AddVersionedOpenApi<TestVersionModelProvider>();

    await using var provider = services.BuildServiceProvider();
    var options = provider.GetRequiredService<IOptionsMonitor<OpenApiOptions>>().Get("v1.0");

    await Assert.That(options.ShouldInclude(DescriptionWithGroup("v1.0"))).IsTrue();
    await Assert.That(options.ShouldInclude(DescriptionWithGroup(string.Empty))).IsTrue();
    await Assert.That(options.ShouldInclude(DescriptionWithGroup("v2.0"))).IsFalse();
  }

  [Test]
  public async Task AddVersionedOpenApi_invokes_user_configuration()
  {
    var services = new ServiceCollection();
    var configured = new List<string>();

    services.AddVersionedOpenApi<TestVersionModelProvider>(_ => configured.Add("configured"));

    await using var provider = services.BuildServiceProvider();
    _ = provider.GetRequiredService<IOptionsMonitor<OpenApiOptions>>().Get("v1.0");

    await Assert.That(configured).Contains("configured");
  }

  // ---- Helpers ----
  private static ApiDescription DescriptionWithGroup(string groupName)
    => new() { GroupName = groupName };

  private sealed class TestVersionModelProvider : IApiVersionModelProvider
  {
    public static ApiVersionModel VersionModel { get; } =
      new([new ApiVersion(1, 0, null), new ApiVersion(2, 0, null)], []);
  }
}
