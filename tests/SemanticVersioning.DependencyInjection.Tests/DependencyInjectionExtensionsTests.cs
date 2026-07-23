namespace Kritikos.SemanticVersioning.DependencyInjection.Tests;

using Microsoft.Extensions.DependencyInjection;

public class DependencyInjectionExtensionsTests
{
  [Test]
  public async Task AddSemanticVersionDescriptor_from_assembly_registers_a_resolvable_descriptor()
  {
    var services = new ServiceCollection();
    var assembly = typeof(SemanticVersionDescriptor).Assembly;

    services.AddSemanticVersionDescriptor(assembly);

    await using var provider = services.BuildServiceProvider();
    var descriptor = provider.GetService<SemanticVersionDescriptor>();

    await Assert.That(descriptor).IsNotNull();
    await Assert.That(descriptor).IsEqualTo(SemanticVersionDescriptor.FromAssembly(assembly));
  }

  [Test]
  public async Task AddSemanticVersionDescriptor_from_type_registers_a_resolvable_descriptor()
  {
    var services = new ServiceCollection();

    services.AddSemanticVersionDescriptor(typeof(SemanticVersionDescriptor));

    await using var provider = services.BuildServiceProvider();

    await Assert.That(provider.GetService<SemanticVersionDescriptor>()).IsNotNull();
  }

  [Test]
  public async Task AddSemanticVersionDescriptor_does_not_overwrite_an_existing_registration()
  {
    var services = new ServiceCollection();
    var existing = SemanticVersionDescriptor.FromInformationalVersion("2.3.4");
    services.AddSingleton(existing);

    services.AddSemanticVersionDescriptor(typeof(SemanticVersionDescriptor).Assembly);

    await using var provider = services.BuildServiceProvider();
    var descriptor = provider.GetRequiredService<SemanticVersionDescriptor>();

    await Assert.That(descriptor).IsEqualTo(existing);
    await Assert.That(descriptor.Major).IsEqualTo(2);
    await Assert.That(descriptor.Minor).IsEqualTo(3);
    await Assert.That(descriptor.Patch).IsEqualTo(4);
  }

  [Test]
  public async Task AddSemanticVersionDescriptor_null_services_throws()
    => await Assert.That(() =>
        ((IServiceCollection)null!).AddSemanticVersionDescriptor(typeof(SemanticVersionDescriptor).Assembly))
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddSemanticVersionDescriptor_null_type_throws()
    => await Assert.That(() => new ServiceCollection().AddSemanticVersionDescriptor((Type)null!))
      .Throws<ArgumentNullException>();
}
