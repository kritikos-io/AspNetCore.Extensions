namespace Kritikos.AspNetCore.SwaggerFeatureManagementOptions;

using System.Reflection;

using Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerFeatureGateFilter(IHttpContextAccessor accessor)
  : IDocumentFilter
{
  private readonly IFeatureManagerSnapshot featureManager = accessor
                                                              .HttpContext?.RequestServices
                                                              .GetRequiredService<IFeatureManagerSnapshot>()
                                                            ?? throw new ArgumentNullException(
                                                              nameof(IFeatureManagerSnapshot));

  public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(swaggerDoc);

    foreach (var apiDescription in context.ApiDescriptions)
    {
      var descriptor = apiDescription.ActionDescriptor;
      var shouldRemoveAction = descriptor switch
      {
        ControllerActionDescriptor controllerActionDescriptor => IsControllerFeatureGateClosed(
          controllerActionDescriptor),
        { } actionDescriptor => IsEndpointFeatureClosed(actionDescriptor).Result,
        _ => throw new ArgumentException(nameof(context.ApiDescriptions)),
      };

      if (!shouldRemoveAction)
      {
        continue;
      }

      var key = $"/{apiDescription.RelativePath}";
      if (Enum.TryParse<OperationType>(apiDescription.HttpMethod, true, out var operation))
      {
        swaggerDoc.Paths[key].Operations.Remove(operation);
      }

      if (!swaggerDoc.Paths[key].Operations.Any())
      {
        swaggerDoc.Paths.Remove(key);
      }
    }
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
      controllerAttributes.Select(
          attribute => attribute.RequirementType switch
          {
            RequirementType.Any => attribute.Features.Any(feature => featureManager.IsEnabledAsync(feature).Result),
            RequirementType.All => attribute.Features.All(feature => featureManager.IsEnabledAsync(feature).Result),
            _ => throw new ArgumentException(nameof(attribute.RequirementType)),
          })
        .ToList();

    var enabledOnActionLevel =
      actionAttributes.Select(
          attribute => attribute.RequirementType switch
          {
            RequirementType.Any => attribute.Features.Any(feature => featureManager.IsEnabledAsync(feature).Result),
            RequirementType.All => attribute.Features.All(feature => featureManager.IsEnabledAsync(feature).Result),
            _ => throw new ArgumentException(nameof(attribute.RequirementType)),
          })
        .ToList();

    return !(enabledOnControllerLevel.All(x => x) && enabledOnActionLevel.All(x => x));
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
}
