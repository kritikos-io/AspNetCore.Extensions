namespace Kritikos.Extensions.Options.DependencyInjection.Tests;

using System.Reflection;

using Kritikos.Extensions.Options.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class OptionsDefinitionRegistrationTests
{
  [Test]
  public async Task AddOptionsDefinitions_binds_configuration_for_discovered_types()
  {
    using var provider = BuildProvider(
      new() { ["Sample:Name"] = "widget", ["Sample:Count"] = "7" },
      services => services.AddOptionsDefinitions(typeof(SampleOptions).Assembly));

    var options = provider.GetRequiredService<IOptions<SampleOptions>>().Value;

    await Assert.That(options.Name).IsEqualTo("widget");
    await Assert.That(options.Count).IsEqualTo(7);
  }

  [Test]
  public async Task AddOptionsDefinitions_enforces_data_annotations_on_resolve()
  {
    using var provider = BuildProvider(
      new() { ["Sample:Name"] = "widget", ["Sample:Count"] = "999" },
      services => services.AddOptionsDefinitions(typeof(SampleOptions).Assembly));

    await Assert.That(() => provider.GetRequiredService<IOptions<SampleOptions>>().Value)
      .Throws<OptionsValidationException>();
  }

  [Test]
  public async Task AddOptionsDefinitions_skips_abstract_definitions()
  {
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

    services.AddOptionsDefinitions(typeof(SampleOptions).Assembly);

    await Assert.That(services.Any(d => d.ServiceType == typeof(IConfigureOptions<SampleOptions>))).IsTrue();
    await Assert.That(services.Any(d => d.ServiceType == typeof(IConfigureOptions<AbstractSampleOptions>))).IsFalse();
  }

  [Test]
  public async Task AddOptionsDefinitions_from_a_marker_type_scans_the_declaring_assembly()
  {
    using var provider = BuildProvider(
      new() { ["Sample:Name"] = "marker", ["Sample:Count"] = "3" },
      services => services.AddOptionsDefinitions(typeof(SampleOptions)));

    var options = provider.GetRequiredService<IOptions<SampleOptions>>().Value;

    await Assert.That(options.Name).IsEqualTo("marker");
  }

  [Test]
  public async Task AddOptionsDefinitions_can_be_invoked_repeatedly()
  {
    using var provider = BuildProvider(
      new() { ["Sample:Name"] = "cached", ["Sample:Count"] = "2" },
      services =>
      {
        services.AddOptionsDefinitions(typeof(SampleOptions).Assembly);
        services.AddOptionsDefinitions(typeof(SampleOptions).Assembly);
      });

    var options = provider.GetRequiredService<IOptions<SampleOptions>>().Value;

    await Assert.That(options.Name).IsEqualTo("cached");
  }

  [Test]
  public async Task AddOptionsDefinition_binds_a_single_options_type()
  {
    using var provider = BuildProvider(
      new() { ["Sample:Name"] = "single", ["Sample:Count"] = "4" },
      services => services.AddOptionsDefinition<SampleOptions>());

    var options = provider.GetRequiredService<IOptions<SampleOptions>>().Value;

    await Assert.That(options.Name).IsEqualTo("single");
    await Assert.That(options.Count).IsEqualTo(4);
  }

  [Test]
  public async Task AddOptionsDefinition_binds_every_named_location()
  {
    using var provider = BuildProvider(
      new()
      {
        ["Named:Primary:Value"] = "first",
        ["Named:Secondary:Value"] = "second",
      },
      services => services.AddOptionsDefinition<NamedSampleOptions>());

    var monitor = provider.GetRequiredService<IOptionsMonitor<NamedSampleOptions>>();

    await Assert.That(monitor.Get("primary").Value).IsEqualTo("first");
    await Assert.That(monitor.Get("secondary").Value).IsEqualTo("second");
  }

  [Test]
  public async Task AddNamedOptionsDefinitions_binds_named_definitions_from_the_assembly()
  {
    using var provider = BuildProvider(
      new()
      {
        ["Named:Primary:Value"] = "alpha",
        ["Named:Secondary:Value"] = "beta",
      },
      services => services.AddNamedOptionsDefinitions(typeof(NamedSampleOptions).Assembly));

    var monitor = provider.GetRequiredService<IOptionsMonitor<NamedSampleOptions>>();

    await Assert.That(monitor.Get("primary").Value).IsEqualTo("alpha");
    await Assert.That(monitor.Get("secondary").Value).IsEqualTo("beta");
  }

  [Test]
  public async Task AddOptionsDefinitions_rejects_a_null_assembly()
    => await Assert.That(() => new ServiceCollection().AddOptionsDefinitions((Assembly)null!))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddOptionsDefinitions_rejects_a_null_marker_type()
    => await Assert.That(() => new ServiceCollection().AddOptionsDefinitions((Type)null!))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddNamedOptionsDefinitions_rejects_a_null_assembly()
    => await Assert.That(() => new ServiceCollection().AddNamedOptionsDefinitions(null!))
      .Throws<ArgumentNullException>();

  private static ServiceProvider BuildProvider(
    Dictionary<string, string?> settings,
    Action<IServiceCollection> configure)
  {
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(settings)
      .Build();

    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(configuration);
    configure(services);

    return services.BuildServiceProvider();
  }
}
