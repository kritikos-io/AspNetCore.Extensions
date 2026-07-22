namespace Kritikos.AspNetCore.FeatureManagementOptionTests;

using Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.FeatureManagement;

public class FeatureGateEndpointFilterTests
{
  private enum SampleFeature
  {
    Alpha,
    Beta,
  }

  [Test]
  public async Task Enum_features_are_stored_as_their_names()
  {
    var filter = new FeatureGateEndpointFilter(RequirementType.Any, SampleFeature.Alpha, SampleFeature.Beta);

    await Assert.That(filter.RequirementType).IsEqualTo(RequirementType.Any);
    await Assert.That(filter.Features).Contains("Alpha");
    await Assert.That(filter.Features).Contains("Beta");
  }

  [Test]
  public async Task Non_enum_features_are_rejected()
  {
    await Assert.That(() => new FeatureGateEndpointFilter(RequirementType.All, new object()))
      .Throws<ArgumentException>();
  }

  [Test]
  public async Task WithFeatureFlags_accepts_enum_values()
  {
    var builder = new FakeEndpointConventionBuilder();

    var result = builder.WithFeatureFlags(RequirementType.All, SampleFeature.Alpha, SampleFeature.Beta);

    await Assert.That(result).IsNotNull();
  }

  private sealed class FakeEndpointConventionBuilder : IEndpointConventionBuilder
  {
    public void Add(Action<EndpointBuilder> convention)
    {
    }

    public void Finally(Action<EndpointBuilder> finallyConvention)
    {
    }
  }
}
