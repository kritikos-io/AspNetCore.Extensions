namespace Kritikos.AspNetCore.VersioningTests;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Builder;

using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;
using Kritikos.AspNetCore.VersioningOptions;

using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class KritikosApiVersioningDependencyInjectionExtensionsTests
{
  [Test]
  public async Task AddApiVersioningDefaults_registers_the_default_options_configurator()
  {
    var services = new ServiceCollection();

    services.AddApiVersioningDefaults(static _ => { });

    await Assert.That(services.Any(static d =>
        d.ServiceType == typeof(IConfigureOptions<ApiVersioningOptions>)
        && d.ImplementationType == typeof(ApiVersioningDefaultOptions))).IsTrue();
  }

  [Test]
  public async Task AddApiVersioningDefaults_null_services_throws()
    => await Assert.That(() => ((IServiceCollection)null!).AddApiVersioningDefaults(static _ => { }))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddApiVersioningDefaults_null_setupAction_throws()
    => await Assert.That(() => new ServiceCollection().AddApiVersioningDefaults(null!))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddApiVersioningDefaults_registers_the_api_catalog_source()
  {
    var services = new ServiceCollection();

    services.AddApiVersioningDefaults(static _ => { });

    await Assert.That(services.Any(static d =>
        d.ServiceType == typeof(IApiCatalogSource)
        && d.ImplementationType == typeof(ApiVersionCatalogSource))).IsTrue();
  }

  [Test]
  public async Task ApiVersionCatalogSource_emits_an_entry_per_api_version()
  {
    var source = new ApiVersionCatalogSource(
      new StubVersionDescriptionProvider(
        new ApiVersionDescription(new ApiVersion(1), "v1"),
        new ApiVersionDescription(new ApiVersion(2), "v2")),
      Options.Create(new ApiVersionCatalogOptions()));

    var entries = source.GetEntries(CreateContext("api.example.com")).ToArray();
    var documents = entries.Select(entry => entry.ServiceDescription!.OriginalString).ToArray();

    await Assert.That(entries.Length).IsEqualTo(2);
    await Assert.That(documents).Contains("https://api.example.com/openapi/v1.json");
    await Assert.That(documents).Contains("https://api.example.com/openapi/v2.json");
    await Assert.That(entries[0].Anchor).IsEqualTo(entries[0].ServiceDescription);
    await Assert.That(entries[1].Anchor).IsEqualTo(entries[1].ServiceDescription);
  }

  [Test]
  public async Task ApiVersionCatalogSource_uses_the_configured_document_route_pattern()
  {
    var source = new ApiVersionCatalogSource(
      new StubVersionDescriptionProvider(new ApiVersionDescription(new ApiVersion(1), "v1")),
      Options.Create(new ApiVersionCatalogOptions { DocumentRoutePattern = "docs/{0}/openapi.json" }));

    var entries = source.GetEntries(CreateContext("api.example.com")).ToArray();

    await Assert.That(entries[0].ServiceDescription!.OriginalString)
      .IsEqualTo("https://api.example.com/docs/v1/openapi.json");
  }

  [Test]
  public async Task ApiVersionCatalogSource_null_context_throws_eagerly()
  {
    var source = new ApiVersionCatalogSource(
      new StubVersionDescriptionProvider(new ApiVersionDescription(new ApiVersion(1), "v1")),
      Options.Create(new ApiVersionCatalogOptions()));

    await Assert.That(() => source.GetEntries(null!)).Throws<ArgumentNullException>();
  }

  [Test]
  public async Task AddApiVersioningDefaults_registers_the_configured_set_and_model()
  {
    var services = new ServiceCollection();

    services.AddApiVersioningDefaults(static set => set.HasApiVersion(new ApiVersion(1)));

    await using var provider = services.BuildServiceProvider();
    await Assert.That(provider.GetService<ApiVersionSet>()).IsNotNull();
    await Assert.That(provider.GetService<ApiVersionModel>()).IsNotNull();
  }

  private static DefaultHttpContext CreateContext(string host)
  {
    var context = new DefaultHttpContext();
    context.Request.Scheme = "https";
    context.Request.Host = new HostString(host);
    return context;
  }

  private sealed class StubVersionDescriptionProvider(params ApiVersionDescription[] descriptions)
    : IApiVersionDescriptionProvider
  {
    public IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions { get; } = descriptions;
  }
}
