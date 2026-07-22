namespace Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;

/// <summary>
/// Extension methods for adding feature flag endpoint filters to endpoint convention builders.
/// </summary>
public static class FeatureEndpointFilterExtensions
{
  /// <summary>
  /// Adds a feature flag filter to the endpoint using enum-based feature names with a specified requirement type.
  /// </summary>
  /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
  /// <typeparam name="TEnum">The enum type representing feature flags.</typeparam>
  /// <param name="builder">The endpoint convention builder.</param>
  /// <param name="requirementType">Whether all or any of the specified features must be enabled.</param>
  /// <param name="features">The feature flags to evaluate.</param>
  /// <returns>The configured <typeparamref name="TBuilder"/>.</returns>
  public static TBuilder WithFeatureFlags<TBuilder, TEnum>(
    this TBuilder builder,
    RequirementType requirementType,
    params TEnum[] features)
    where TBuilder : IEndpointConventionBuilder
    where TEnum : Enum
  {
    ArgumentNullException.ThrowIfNull(builder);

    var filter = new FeatureGateEndpointFilter(requirementType, features.Cast<object>().ToArray());
    builder.AddEndpointFilter(filter);
    builder.WithMetadata(filter);

    return builder;
  }

  /// <summary>
  /// Adds a feature flag filter to the endpoint using string-based feature names with a specified requirement type.
  /// </summary>
  /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
  /// <param name="builder">The endpoint convention builder.</param>
  /// <param name="requirementType">Whether all or any of the specified features must be enabled.</param>
  /// <param name="features">The feature flag names to evaluate.</param>
  /// <returns>The configured <typeparamref name="TBuilder"/>.</returns>
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

  /// <summary>
  /// Adds a feature flag filter to the endpoint using enum-based feature names, requiring all features to be enabled.
  /// </summary>
  /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
  /// <typeparam name="TEnum">The enum type representing feature flags.</typeparam>
  /// <param name="builder">The endpoint convention builder.</param>
  /// <param name="features">The feature flags to evaluate.</param>
  /// <returns>The configured <typeparamref name="TBuilder"/>.</returns>
  public static TBuilder WithFeatureFlags<TBuilder, TEnum>(
    this TBuilder builder,
    params TEnum[] features)
    where TBuilder : IEndpointConventionBuilder
    where TEnum : Enum
    => builder.WithFeatureFlags(RequirementType.All, features);

  /// <summary>
  /// Adds a feature flag filter to the endpoint using string-based feature names, requiring all features to be enabled.
  /// </summary>
  /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
  /// <param name="builder">The endpoint convention builder.</param>
  /// <param name="features">The feature flag names to evaluate.</param>
  /// <returns>The configured <typeparamref name="TBuilder"/>.</returns>
  public static TBuilder WithFeatureFlags<TBuilder>(
    this TBuilder builder,
    params string[] features)
    where TBuilder : IEndpointConventionBuilder
    => builder.WithFeatureFlags(RequirementType.All, features);
}
