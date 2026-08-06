namespace Kritikos.AspNetCore.OpenApiExtensions.Tests;

using System.Reflection;

using Kritikos.AspNetCore.OpenApiExtensions.OperationTransformers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using NSubstitute;

public class AuthorizationCheckOperationTransformerTests
{
  [Test]
  public async Task TransformAsync_null_operation_throws()
  {
    var transformer = new AuthorizationCheckOperationTransformer();

    await Assert.That(async () => await transformer.TransformAsync(null!, CreateContext(), CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task TransformAsync_null_context_throws()
  {
    var transformer = new AuthorizationCheckOperationTransformer();

    await Assert.That(async () => await transformer.TransformAsync(new OpenApiOperation(), null!, CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task Authorized_operation_gets_security_and_responses()
  {
    var transformer = new AuthorizationCheckOperationTransformer();
    var operation = new OpenApiOperation();
    var context = CreateContext(new AuthorizeAttribute(), GetActionMethod());

    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(operation.OperationId).IsEqualTo(nameof(SampleController.Secured));
    await Assert.That(operation.Responses!.ContainsKey("401")).IsTrue();
    await Assert.That(operation.Responses!.ContainsKey("403")).IsTrue();
    await Assert.That(operation.Security!.Count).IsEqualTo(1);
  }

  [Test]
  public async Task Unauthorized_operation_is_left_untouched()
  {
    var transformer = new AuthorizationCheckOperationTransformer();
    var operation = new OpenApiOperation();
    var context = CreateContext(GetActionMethod());

    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(operation.OperationId).IsEqualTo(nameof(SampleController.Secured));
    await Assert.That(operation.Security is null || operation.Security.Count == 0).IsTrue();
    await Assert.That(operation.Responses is null || !operation.Responses.ContainsKey("401")).IsTrue();
  }

  [Test]
  public async Task AllowAnonymous_operation_is_left_untouched()
  {
    var transformer = new AuthorizationCheckOperationTransformer();
    var operation = new OpenApiOperation();
    var context = CreateContext(new AuthorizeAttribute(), new AllowAnonymousAttribute(), GetActionMethod());

    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(operation.OperationId).IsEqualTo(nameof(SampleController.Secured));
    await Assert.That(operation.Security is null || operation.Security.Count == 0).IsTrue();
    await Assert.That(operation.Responses is null || !operation.Responses.ContainsKey("401")).IsTrue();
  }

  [Test]
  public async Task Existing_operation_id_is_preserved()
  {
    var transformer = new AuthorizationCheckOperationTransformer();
    var operation = new OpenApiOperation { OperationId = "custom" };
    var context = CreateContext(new AuthorizeAttribute(), GetActionMethod());

    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(operation.OperationId).IsEqualTo("custom");
  }

  [Test]
  public async Task Authorized_operation_without_derivable_id_is_skipped()
  {
    var transformer = new AuthorizationCheckOperationTransformer();
    var operation = new OpenApiOperation();
    var context = CreateContext(new AuthorizeAttribute());

    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(operation.OperationId).IsNull();
    await Assert.That(operation.Security is null || operation.Security.Count == 0).IsTrue();
  }

  [Test]
  public async Task Repeated_transforms_do_not_duplicate_responses()
  {
    var transformer = new AuthorizationCheckOperationTransformer();
    var operation = new OpenApiOperation();
    var context = CreateContext(new AuthorizeAttribute(), GetActionMethod());

    await transformer.TransformAsync(operation, context, CancellationToken.None);
    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(operation.Responses!.Count).IsEqualTo(2);
  }

  [Test]
  public async Task Attribute_authentication_scheme_is_mapped_to_configured_key()
  {
    var transformer = new AuthorizationCheckOperationTransformer(
      "oauth2",
      new Dictionary<string, string>(StringComparer.Ordinal) { ["ApiKey"] = "apiKey" });
    var operation = new OpenApiOperation();
    var context = CreateContext(new AuthorizeAttribute { AuthenticationSchemes = "ApiKey" }, GetActionMethod());

    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(ReferencedSchemeIds(operation)).IsEquivalentTo(["apiKey"]);
  }

  [Test]
  public async Task Policy_authentication_scheme_is_mapped_to_configured_key()
  {
    var transformer = new AuthorizationCheckOperationTransformer(
      "oauth2",
      new Dictionary<string, string>(StringComparer.Ordinal) { ["ApiKey"] = "apiKey" });
    var operation = new OpenApiOperation();
    var policy = new AuthorizationPolicyBuilder()
      .AddAuthenticationSchemes("ApiKey")
      .RequireAssertion(_ => true)
      .Build();
    var context = CreateContext(new AuthorizeAttribute(), policy, GetActionMethod());

    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(ReferencedSchemeIds(operation)).IsEquivalentTo(["apiKey"]);
  }

  [Test]
  public async Task Operation_without_authentication_scheme_uses_default_key()
  {
    var transformer = new AuthorizationCheckOperationTransformer(
      "oauth2",
      new Dictionary<string, string>(StringComparer.Ordinal) { ["ApiKey"] = "apiKey" });
    var operation = new OpenApiOperation();
    var context = CreateContext(new AuthorizeAttribute(), GetActionMethod());

    await transformer.TransformAsync(operation, context, CancellationToken.None);

    await Assert.That(ReferencedSchemeIds(operation)).IsEquivalentTo(["oauth2"]);
  }

  // ---- Helpers ----
  private static MethodInfo GetActionMethod()
    => typeof(SampleController).GetMethod(nameof(SampleController.Secured))!;

  private static IReadOnlyList<string> ReferencedSchemeIds(OpenApiOperation operation)
    => [.. operation.Security!
      .SelectMany(requirement => requirement.Keys)
      .OfType<OpenApiSecuritySchemeReference>()
      .Select(reference => reference.Reference.Id!),];

  private static OpenApiOperationTransformerContext CreateContext(params object[] metadata)
    => new()
    {
      DocumentName = "v1",
      Description = new ApiDescription
      {
        ActionDescriptor = new ActionDescriptor { EndpointMetadata = [.. metadata] },
      },
      ApplicationServices = Substitute.For<IServiceProvider>(),
    };

  private sealed class SampleController
  {
    public void Secured()
    {
    }
  }
}
