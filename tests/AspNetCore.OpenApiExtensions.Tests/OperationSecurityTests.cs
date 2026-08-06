namespace Kritikos.AspNetCore.OpenApiExtensions.Tests;

using Microsoft.AspNetCore.Authorization;

using Microsoft.OpenApi;

public class OperationSecurityTests
{
  [Test]
  public async Task IsSecured_is_true_for_authorize_without_anonymous()
  {
    var secured = OperationSecurity.IsSecured([new AuthorizeAttribute()], out var authorization);

    await Assert.That(secured).IsTrue();
    await Assert.That(authorization.Length).IsEqualTo(1);
  }

  [Test]
  public async Task IsSecured_is_false_when_allow_anonymous_present()
  {
    var secured = OperationSecurity.IsSecured(
      [new AuthorizeAttribute(), new AllowAnonymousAttribute()], out _);

    await Assert.That(secured).IsFalse();
  }

  [Test]
  public async Task IsSecured_is_false_without_authorize_data()
  {
    var secured = OperationSecurity.IsSecured([new object()], out var authorization);

    await Assert.That(secured).IsFalse();
    await Assert.That(authorization).IsEmpty();
  }

  [Test]
  public async Task Apply_adds_responses_and_maps_the_authentication_scheme()
  {
    var operation = new OpenApiOperation();
    var document = new OpenApiDocument();
    object[] metadata = [new AuthorizeAttribute { AuthenticationSchemes = "ApiKey" }];
    OperationSecurity.IsSecured(metadata, out var authorization);

    OperationSecurity.Apply(
      operation,
      document,
      metadata,
      authorization,
      "oauth2",
      new Dictionary<string, string>(StringComparer.Ordinal) { ["ApiKey"] = "apiKey" });

    await Assert.That(operation.Responses!.ContainsKey("401")).IsTrue();
    await Assert.That(operation.Responses!.ContainsKey("403")).IsTrue();
    await Assert.That(ReferencedSchemeIds(operation)).IsEquivalentTo(["apiKey"]);
  }

  [Test]
  public async Task Apply_falls_back_to_the_default_scheme()
  {
    var operation = new OpenApiOperation();
    var document = new OpenApiDocument();
    object[] metadata = [new AuthorizeAttribute()];
    OperationSecurity.IsSecured(metadata, out var authorization);

    OperationSecurity.Apply(
      operation,
      document,
      metadata,
      authorization,
      "oauth2",
      new Dictionary<string, string>(StringComparer.Ordinal));

    await Assert.That(ReferencedSchemeIds(operation)).IsEquivalentTo(["oauth2"]);
  }

  private static IReadOnlyList<string> ReferencedSchemeIds(OpenApiOperation operation)
    => [.. operation.Security!
      .SelectMany(requirement => requirement.Keys)
      .OfType<OpenApiSecuritySchemeReference>()
      .Select(reference => reference.Reference.Id!),];
}
