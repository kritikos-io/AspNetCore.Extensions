namespace Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

/// <summary>
/// An endpoint filter that gates access based on the state of one or more feature flags.
/// </summary>
public class FeatureGateEndpointFilter
  : IEndpointFilter
{
  /// <summary>
  /// Initializes a new instance of the <see cref="FeatureGateEndpointFilter"/> class requiring all specified string features.
  /// </summary>
  /// <param name="features">The feature names to evaluate.</param>
  public FeatureGateEndpointFilter(params string[] features)
    : this(RequirementType.All, features)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FeatureGateEndpointFilter"/> class requiring all specified enum features.
  /// </summary>
  /// <param name="features">The enum feature values to evaluate.</param>
  public FeatureGateEndpointFilter(params object[] features)
    : this(RequirementType.All, features)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FeatureGateEndpointFilter"/> class with enum features and a specified requirement type.
  /// </summary>
  /// <param name="requirementType">Whether all or any of the specified features must be enabled.</param>
  /// <param name="features">The enum feature values to evaluate.</param>
  public FeatureGateEndpointFilter(RequirementType requirementType, params object[] features)
  {
    if (features == null || features.Length == 0)
    {
      throw new ArgumentNullException(nameof(features), "Features can not be null or empty!");
    }

    if (!features.GetType().IsEnum)
    {
      throw new ArgumentException("The provided features must be enums.", nameof(features));
    }

    List<string> featureStrings =
    [
      .. features
        .Select(static feature => Enum.GetName(feature.GetType(), feature))
        .OfType<string>()
        .Where(static x => !string.IsNullOrWhiteSpace(x)),
    ];

    Features = featureStrings;
    RequirementType = requirementType;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FeatureGateEndpointFilter"/> class with string features and a specified requirement type.
  /// </summary>
  /// <param name="requirementType">Whether all or any of the specified features must be enabled.</param>
  /// <param name="features">The feature names to evaluate.</param>
  public FeatureGateEndpointFilter(RequirementType requirementType, params string[] features)
  {
    if (features == null || features.Length == 0 || features.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentNullException(nameof(features), "Features can not be null or whitespace!");
    }

    Features = [.. features];
    RequirementType = requirementType;
  }

  /// <summary>
  /// Gets the feature flag names that this filter evaluates.
  /// </summary>
  public IEnumerable<string> Features { get; }

  /// <summary>
  /// Gets the requirement mode that determines whether all or any of the <see cref="Features"/> must be enabled to pass.
  /// </summary>
  public RequirementType RequirementType { get; }

  /// <inheritdoc />
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(next);

    var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManagerSnapshot>()
                         ?? throw new ArgumentException(nameof(IFeatureManagerSnapshot));
    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>()
                                ?? throw new ArgumentException(nameof(ProblemDetailsFactory));

    var isEnabled = RequirementType == RequirementType.All;
    foreach (var feature in Features)
    {
      var isFeatureEnabled = await featureManager.IsEnabledAsync(feature);
      isEnabled = RequirementType switch
      {
        RequirementType.All => isEnabled && isFeatureEnabled,
        RequirementType.Any => isEnabled || isFeatureEnabled,
        _ => throw new ArgumentException(nameof(RequirementType)),
      };
    }

    if (isEnabled)
    {
      return await next(context);
    }

    context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
    return problemDetailsFactory.CreateProblemDetails(
      context.HttpContext,
      StatusCodes.Status404NotFound,
      title: "Not Found");
  }
}
