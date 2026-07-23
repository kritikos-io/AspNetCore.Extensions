namespace Kritikos.AspNetCore.VersioningTests;

using Asp.Versioning;
using Asp.Versioning.Builder;

using Kritikos.AspNetCore.VersioningOptions;
using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class KritikosApiVersioningDependencyInjectionExtensionsTests
{
  [Test]
  public async Task AddApiVersioningDefaults_registers_the_default_options_configurator()
  {
    var services = new ServiceCollection();

    services.AddApiVersioningDefaults();

    await Assert.That(services.Any(static d =>
        d.ServiceType == typeof(IConfigureOptions<ApiVersioningOptions>)
        && d.ImplementationType == typeof(ApiVersioningDefaultOptions))).IsTrue();
  }

  [Test]
  public async Task AddApiVersioningDefaults_null_services_throws()
    => await Assert.That(() => ((IServiceCollection)null!).AddApiVersioningDefaults())
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddApiVersionModelProvider_registers_the_version_model()
  {
    var services = new ServiceCollection();

    services.AddApiVersionModelProvider<TestVersionModelProvider>();

    await using var provider = services.BuildServiceProvider();

    await Assert.That(provider.GetRequiredService<ApiVersionModel>())
      .IsEqualTo(TestVersionModelProvider.VersionModel);
  }

  [Test]
  public async Task AddApiVersionModelProvider_null_services_throws()
    => await Assert.That(() =>
        ((IServiceCollection)null!).AddApiVersionModelProvider<TestVersionModelProvider>())
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddApiVersionSetProvider_registers_the_version_set()
  {
    var services = new ServiceCollection();

    services.AddApiVersionSetProvider<TestVersionSetProvider>();

    await Assert.That(services.Any(static d => d.ServiceType == typeof(ApiVersionSet))).IsTrue();
  }

  [Test]
  public async Task AddApiVersionSetProvider_null_services_throws()
    => await Assert.That(() =>
        ((IServiceCollection)null!).AddApiVersionSetProvider<TestVersionSetProvider>())
      .Throws<ArgumentNullException>();

  private sealed class TestVersionModelProvider : IApiVersionModelProvider
  {
    public static ApiVersionModel VersionModel { get; } = new(new ApiVersion(1));
  }

  private sealed class TestVersionSetProvider : IApiVersionSetProvider
  {
    public static ApiVersionSet VersionSet { get; set; } = null!;
  }
}
