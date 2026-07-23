namespace Kritikos.AspNetCore.OpenApiFeatureManagementOptions.Tests;

using System.Reflection;

using Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi;

using NSubstitute;

public class FeatureFilterDocumentTransformerTests
{
  // ---- Argument validation ----
  [Test]
  public async Task TransformAsync_null_document_throws()
  {
    var transformer = new FeatureFilterDocumentTransformer(Substitute.For<IFeatureManager>());
    var context = CreateContext(CreateEndpointDescription("weather", "GET"));

    await Assert.That(async () => await transformer.TransformAsync(null!, context, CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task TransformAsync_null_context_throws()
  {
    var transformer = new FeatureFilterDocumentTransformer(Substitute.For<IFeatureManager>());
    var document = CreateDocument("weather", HttpMethod.Get);

    await Assert.That(async () => await transformer.TransformAsync(document, null!, CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  // ---- Minimal API endpoint gating ----
  [Test]
  public async Task Endpoint_with_disabled_feature_is_removed()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsFalse();
  }

  [Test]
  public async Task Endpoint_with_enabled_feature_is_kept()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsTrue();
  }

  [Test]
  public async Task Endpoint_without_feature_gate_is_kept()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var context = CreateContext(CreateEndpointDescription("weather", "GET"));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsTrue();
  }

  [Test]
  public async Task Endpoint_with_any_requirement_is_kept_when_one_feature_is_enabled()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    featureManager.IsEnabledAsync("Beta").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var gate = new FeatureGateEndpointFilter(RequirementType.Any, "Alpha", "Beta");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsTrue();
  }

  [Test]
  public async Task Endpoint_with_any_requirement_is_removed_when_all_features_are_disabled()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    featureManager.IsEnabledAsync("Beta").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var gate = new FeatureGateEndpointFilter(RequirementType.Any, "Alpha", "Beta");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsFalse();
  }

  [Test]
  public async Task Disabled_operation_is_removed_while_sibling_operation_on_same_path_is_kept()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    // Same path exposes GET (gated, disabled) and POST (ungated).
    var document = CreateDocument("weather", HttpMethod.Get, HttpMethod.Post);
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsTrue();
    var operations = document.Paths["/weather"].Operations!;
    await Assert.That(operations.ContainsKey(HttpMethod.Get)).IsFalse();
    await Assert.That(operations.ContainsKey(HttpMethod.Post)).IsTrue();
  }

  [Test]
  public async Task Closed_feature_with_no_matching_path_does_not_throw()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = new OpenApiDocument { Paths = [] };
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.Count).IsEqualTo(0);
  }

  [Test]
  public async Task Endpoint_requiring_all_features_is_removed_when_one_is_disabled()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    featureManager.IsEnabledAsync("Beta").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha", "Beta");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsFalse();
  }

  [Test]
  public async Task Endpoint_requiring_all_features_is_kept_when_every_feature_is_enabled()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    featureManager.IsEnabledAsync("Beta").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha", "Beta");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsTrue();
  }

  [Test]
  public async Task Endpoint_with_multiple_gates_is_kept_when_all_gates_are_open()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    featureManager.IsEnabledAsync("Beta").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var firstGate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var secondGate = new FeatureGateEndpointFilter(RequirementType.All, "Beta");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", firstGate, secondGate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsTrue();
  }

  [Test]
  public async Task Endpoint_with_multiple_gates_is_removed_when_any_gate_is_closed()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    featureManager.IsEnabledAsync("Beta").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get);
    var openGate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var closedGate = new FeatureGateEndpointFilter(RequirementType.All, "Beta");
    var context = CreateContext(CreateEndpointDescription("weather", "GET", openGate, closedGate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsFalse();
  }

  [Test]
  public async Task Path_is_removed_when_all_of_its_operations_are_disabled()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("weather", HttpMethod.Get, HttpMethod.Post);
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var context = CreateContext(
      CreateEndpointDescription("weather", "GET", gate),
      CreateEndpointDescription("weather", "POST", gate));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsFalse();
  }

  // ---- Controller / action attribute gating ----
  [Test]
  public async Task Controller_with_disabled_controller_gate_is_removed()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(CreateControllerDescription<GatedController>("pets", "GET", nameof(GatedController.List)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsFalse();
  }

  [Test]
  public async Task Controller_with_enabled_controller_gate_is_kept()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(CreateControllerDescription<GatedController>("pets", "GET", nameof(GatedController.List)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsTrue();
  }

  [Test]
  public async Task Action_with_disabled_gate_is_removed()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Beta").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(CreateControllerDescription<PlainController>("pets", "GET", nameof(PlainController.Gated)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsFalse();
  }

  [Test]
  public async Task Action_with_any_requirement_is_kept_when_one_feature_is_enabled()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    featureManager.IsEnabledAsync("Beta").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(CreateControllerDescription<PlainController>("pets", "GET", nameof(PlainController.AnyGated)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsTrue();
  }

  [Test]
  public async Task Controller_action_without_feature_gate_is_kept()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(CreateControllerDescription<PlainController>("pets", "GET", nameof(PlainController.List)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsTrue();
  }

  [Test]
  public async Task Controller_and_action_gates_both_enabled_keeps_operation()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    featureManager.IsEnabledAsync("Beta").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(
      CreateControllerDescription<DoublyGatedController>("pets", "GET", nameof(DoublyGatedController.List)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsTrue();
  }

  [Test]
  public async Task Controller_gate_enabled_but_action_gate_disabled_removes_operation()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    featureManager.IsEnabledAsync("Beta").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(
      CreateControllerDescription<DoublyGatedController>("pets", "GET", nameof(DoublyGatedController.List)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsFalse();
  }

  [Test]
  public async Task Action_gate_enabled_but_controller_gate_disabled_removes_operation()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    featureManager.IsEnabledAsync("Beta").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(
      CreateControllerDescription<DoublyGatedController>("pets", "GET", nameof(DoublyGatedController.List)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsFalse();
  }

  [Test]
  public async Task Action_requiring_all_features_is_removed_when_one_is_disabled()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(true);
    featureManager.IsEnabledAsync("Beta").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocument("pets", HttpMethod.Get);
    var context = CreateContext(
      CreateControllerDescription<PlainController>("pets", "GET", nameof(PlainController.AllGated)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/pets")).IsFalse();
  }

  // ---- Multiple descriptions ----
  [Test]
  public async Task Only_the_disabled_description_is_removed_from_the_document()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    featureManager.IsEnabledAsync("Beta").Returns(true);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocumentWithPaths("weather", "pets");
    var closed = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var open = new FeatureGateEndpointFilter(RequirementType.All, "Beta");
    var context = CreateContext(
      CreateEndpointDescription("weather", "GET", closed),
      CreateEndpointDescription("pets", "GET", open));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsFalse();
    await Assert.That(document.Paths.ContainsKey("/pets")).IsTrue();
  }

  [Test]
  public async Task Disabled_controller_and_endpoint_descriptions_are_both_removed()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocumentWithPaths("weather", "pets");
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var context = CreateContext(
      CreateEndpointDescription("weather", "GET", gate),
      CreateControllerDescription<GatedController>("pets", "GET", nameof(GatedController.List)));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsFalse();
    await Assert.That(document.Paths.ContainsKey("/pets")).IsFalse();
  }

  [Test]
  public async Task Descriptions_from_multiple_groups_are_all_processed()
  {
    var featureManager = Substitute.For<IFeatureManager>();
    featureManager.IsEnabledAsync("Alpha").Returns(false);
    var transformer = new FeatureFilterDocumentTransformer(featureManager);

    var document = CreateDocumentWithPaths("weather", "pets");
    var gate = new FeatureGateEndpointFilter(RequirementType.All, "Alpha");
    var context = CreateContextFromGroups(
      new ApiDescriptionGroup("weather", [CreateEndpointDescription("weather", "GET", gate)]),
      new ApiDescriptionGroup("pets", [CreateEndpointDescription("pets", "GET", gate)]));

    await transformer.TransformAsync(document, context, CancellationToken.None);

    await Assert.That(document.Paths.ContainsKey("/weather")).IsFalse();
    await Assert.That(document.Paths.ContainsKey("/pets")).IsFalse();
  }

  // ---- Helpers ----
  private static OpenApiDocument CreateDocument(string relativePath, params HttpMethod[] methods)
  {
    var operations = new Dictionary<HttpMethod, OpenApiOperation>();
    foreach (var method in methods)
    {
      operations[method] = new OpenApiOperation();
    }

    return new OpenApiDocument
    {
      Paths = new OpenApiPaths
      {
        ["/" + relativePath] = new OpenApiPathItem { Operations = operations },
      },
    };
  }

  private static OpenApiDocument CreateDocumentWithPaths(params string[] relativePaths)
  {
    var paths = new OpenApiPaths();
    foreach (var relativePath in relativePaths)
    {
      paths["/" + relativePath] = new OpenApiPathItem
      {
        Operations = new Dictionary<HttpMethod, OpenApiOperation> { [HttpMethod.Get] = new OpenApiOperation() },
      };
    }

    return new OpenApiDocument { Paths = paths };
  }

  private static OpenApiDocumentTransformerContext CreateContext(params ApiDescription[] descriptions)
    => CreateContextFromGroups(new ApiDescriptionGroup("group", descriptions));

  private static OpenApiDocumentTransformerContext CreateContextFromGroups(params ApiDescriptionGroup[] groups)
    => new()
    {
      DocumentName = "v1",
      DescriptionGroups = groups,
      ApplicationServices = Substitute.For<IServiceProvider>(),
    };

  private static ApiDescription CreateEndpointDescription(
    string relativePath,
    string httpMethod,
    params object[] metadata)
    => new()
    {
      ActionDescriptor = new ActionDescriptor { EndpointMetadata = [.. metadata] },
      HttpMethod = httpMethod,
      RelativePath = relativePath,
    };

  private static ApiDescription CreateControllerDescription<TController>(
    string relativePath,
    string httpMethod,
    string actionName)
    => new()
    {
      ActionDescriptor = new ControllerActionDescriptor
      {
        ControllerTypeInfo = typeof(TController).GetTypeInfo(),
        MethodInfo = typeof(TController).GetMethod(actionName)!,
        EndpointMetadata = [],
      },
      HttpMethod = httpMethod,
      RelativePath = relativePath,
    };

  [FeatureGate("Alpha")]
  private sealed class GatedController
  {
    public void List()
    {
    }
  }

  private sealed class PlainController
  {
    public void List()
    {
    }

    [FeatureGate("Beta")]
    public void Gated()
    {
    }

    [FeatureGate(RequirementType.Any, "Alpha", "Beta")]
    public void AnyGated()
    {
    }

    [FeatureGate(RequirementType.All, "Alpha", "Beta")]
    public void AllGated()
    {
    }
  }

  [FeatureGate("Alpha")]
  private sealed class DoublyGatedController
  {
    [FeatureGate("Beta")]
    public void List()
    {
    }
  }
}
