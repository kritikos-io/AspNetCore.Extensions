namespace Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;

public static class FeatureEndpointFilterExtensions
{
  public static TBuilder WithFeatureFlags<TBuilder, TEnum>(
    this TBuilder builder,
    RequirementType requirementType,
    params TEnum[] features)
    where TBuilder : IEndpointConventionBuilder
    where TEnum : Enum
  {
    ArgumentNullException.ThrowIfNull(builder);

    var filter = new FeatureGateEndpointFilter(requirementType, features);
    builder.AddEndpointFilter(filter);
    builder.WithMetadata(filter);

    return builder;
  }

  public static TBuilder WithFeatureFlags<TBuilder>(
    this TBuilder builder,
    RequirementType requirementType,
    params string[] features)
    where TBuilder : IEndpointConventionBuilder
  {
    ArgumentNullException.ThrowIfNull(builder);

    var filter = new FeatureGateEndpointFilter(requirementType, features);
    builder.AddEndpointFilter(filter);
    builder.WithMetadata(filter);

    return builder;
  }

  public static TBuilder WithFeatureFlags<TBuilder, TEnum>(
    this TBuilder builder,
    params TEnum[] features)
    where TBuilder : IEndpointConventionBuilder
    where TEnum : Enum
    => builder.WithFeatureFlags(RequirementType.All, features);

  public static TBuilder WithFeatureFlags<TBuilder>(
    this TBuilder builder,
    params string[] features)
    where TBuilder : IEndpointConventionBuilder
    => builder.WithFeatureFlags(RequirementType.All, features);
}
