namespace Kritikos.AspNetCore.OpenApiFeatureManagementOptions;

using System.Reflection;

using Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi;

/// <summary>
/// An OpenAPI document transformer that removes operations gated behind disabled feature flags.
/// </summary>
/// <param name="featureManager">The feature manager used to evaluate feature flag state.</param>
public sealed class FeatureFilterDocumentTransformer(IFeatureManager featureManager)
  : IOpenApiDocumentTransformer
{
  private readonly IFeatureManager featureManager = featureManager;

  /// <inheritdoc />
  public async Task TransformAsync(
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(document);
    ArgumentNullException.ThrowIfNull(context);

    foreach (var apiDescription in context.DescriptionGroups.SelectMany(static x => x.Items))
    {
      var descriptor = apiDescription.ActionDescriptor;
      var shouldRemoveAction = descriptor switch
      {
        ControllerActionDescriptor controllerActionDescriptor => await IsControllerFeatureGateClosed(
          controllerActionDescriptor),
        { } actionDescriptor => await IsEndpointFeatureClosed(actionDescriptor),
        _ => throw new ArgumentException(nameof(context.DescriptionGroups)),
      };

      if (!shouldRemoveAction)
      {
        continue;
      }

      var key = $"/{apiDescription.RelativePath}";
      if (document.Paths is not null
          && document.Paths.TryGetValue(key, out var path)
          && apiDescription.HttpMethod is not null
          && path.Operations is not null)
      {
        path.Operations.Remove(new HttpMethod(apiDescription.HttpMethod));

        if (path.Operations.Count == 0)
        {
          document.Paths.Remove(key);
        }
      }
    }
  }

  private async Task<bool> IsEndpointFeatureClosed(ActionDescriptor descriptor)
  {
    var metadata = descriptor.EndpointMetadata.ToList();
    var gates = metadata.OfType<FeatureGateEndpointFilter>().ToList();

    var shouldRemove = false;
    foreach (var gate in gates)
    {
      var features = gate.Features.ToList();

      var gateEnabled = gate.RequirementType == RequirementType.All;
      foreach (var feature in features)
      {
        gateEnabled = gate.RequirementType switch
        {
          RequirementType.All => gateEnabled && await featureManager.IsEnabledAsync(feature),
          RequirementType.Any => gateEnabled || await featureManager.IsEnabledAsync(feature),
          _ => throw new ArgumentException(nameof(gate.RequirementType)),
        };
      }

      shouldRemove |= !gateEnabled;
    }

    return shouldRemove;
  }

  private async Task<bool> IsControllerFeatureGateClosed(ControllerActionDescriptor descriptor)
  {
    var controllerAttributes = descriptor.ControllerTypeInfo
      .GetCustomAttributes<FeatureGateAttribute>()
      .ToList();
    var actionAttributes = descriptor.MethodInfo
      .GetCustomAttributes<FeatureGateAttribute>()
      .ToList();

    if (actionAttributes.Count == 0 && controllerAttributes.Count == 0)
    {
      return false;
    }

    var controllerEnabled = await AreAllGatesEnabled(controllerAttributes);
    var actionEnabled = await AreAllGatesEnabled(actionAttributes);

    return !(controllerEnabled && actionEnabled);
  }

  private async Task<bool> AreAllGatesEnabled(IEnumerable<FeatureGateAttribute> attributes)
  {
    foreach (var attribute in attributes)
    {
      var gateEnabled = attribute.RequirementType == RequirementType.All;
      foreach (var feature in attribute.Features)
      {
        gateEnabled = attribute.RequirementType switch
        {
          RequirementType.All => gateEnabled && await featureManager.IsEnabledAsync(feature),
          RequirementType.Any => gateEnabled || await featureManager.IsEnabledAsync(feature),
          _ => throw new ArgumentException(nameof(attribute.RequirementType)),
        };
      }

      if (!gateEnabled)
      {
        return false;
      }
    }

    return true;
  }
}
