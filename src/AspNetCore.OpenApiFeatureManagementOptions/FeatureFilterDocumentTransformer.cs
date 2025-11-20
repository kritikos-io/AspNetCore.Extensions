namespace Kritikos.AspNetCore.OpenApiFeatureManagementOptions;

using System.Reflection;

using Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi.Models;

public class FeatureFilterDocumentTransformer(IFeatureManager featureManager)
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
        ControllerActionDescriptor controllerActionDescriptor => IsControllerFeatureGateClosed(
          controllerActionDescriptor),
        { } actionDescriptor => await IsEndpointFeatureClosed(actionDescriptor),
        _ => throw new ArgumentException(nameof(context.DescriptionGroups)),
      };

      if (!shouldRemoveAction)
      {
        continue;
      }

      var key = $"/{apiDescription.RelativePath}";
      if (document.Paths.TryGetValue(key, out var path)
          && Enum.TryParse<OperationType>(apiDescription.HttpMethod, true, out var operation))
      {
        path.Operations.Remove(operation);
      }

      if (path != null && !path.Operations.Any())
      {
        document.Paths.Remove(key);
      }
    }
  }

  private async Task<bool> IsEndpointFeatureClosed(ActionDescriptor descriptor)
  {
    var metadata = descriptor.EndpointMetadata.ToList();
    var gates = metadata.OfType<FeatureGateEndpointFilter>().ToList();
    var isEnabled = gates.Count > 0;

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

      isEnabled &= !gateEnabled;
    }

    return isEnabled;
  }

  private bool IsControllerFeatureGateClosed(ControllerActionDescriptor descriptor)
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

    var enabledOnControllerLevel =
      controllerAttributes.Select(attribute => attribute.RequirementType switch
        {
          RequirementType.Any => attribute.Features.Any(feature => featureManager.IsEnabledAsync(feature).Result),
          RequirementType.All => attribute.Features.All(feature => featureManager.IsEnabledAsync(feature).Result),
          _ => throw new ArgumentException(nameof(attribute.RequirementType)),
        })
        .ToList();

    var enabledOnActionLevel =
      actionAttributes.Select(attribute => attribute.RequirementType switch
        {
          RequirementType.Any => attribute.Features.Any(feature => featureManager.IsEnabledAsync(feature).Result),
          RequirementType.All => attribute.Features.All(feature => featureManager.IsEnabledAsync(feature).Result),
          _ => throw new ArgumentException(nameof(attribute.RequirementType)),
        })
        .ToList();

    return !(enabledOnControllerLevel.All(x => x) && enabledOnActionLevel.All(x => x));
  }
}
