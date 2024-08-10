namespace Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

public class FeatureGateEndpointFilter
  : IEndpointFilter
{
  public FeatureGateEndpointFilter(params string[] features)
    : this(RequirementType.All, features)
  {
  }

  public FeatureGateEndpointFilter(params object[] features)
    : this(RequirementType.All, features)
  {
  }

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

    List<string> featureStrings = [];
    featureStrings.AddRange(features
      .Select(feature => Enum.GetName(feature.GetType(), feature))
      .OfType<string>()
      .Where(x => !string.IsNullOrWhiteSpace(x))
      .ToList());

    Features = featureStrings;
    RequirementType = requirementType;
  }

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
  /// The name of the features that the feature attribute will activate for.
  /// </summary>
  public IEnumerable<string> Features { get; }

  /// <summary>
  /// Controls whether any or all features in <see cref="Microsoft.FeatureManagement.Mvc.FeatureGateAttribute.Features" /> should be enabled to pass.
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
      var features = await featureManager.IsEnabledAsync(feature);
      isEnabled = RequirementType switch
      {
        RequirementType.All => isEnabled && await featureManager.IsEnabledAsync(feature),
        RequirementType.Any => isEnabled || await featureManager.IsEnabledAsync(feature),
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
