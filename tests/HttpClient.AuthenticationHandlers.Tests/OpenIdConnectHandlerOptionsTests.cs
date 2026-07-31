namespace Kritikos.HttpClient.AuthenticationHandlers.Tests;

using System.ComponentModel.DataAnnotations;

using Kritikos.HttpClient.AuthenticationHandlers;

public sealed class OpenIdConnectHandlerOptionsTests
{
  [Test]
  public async Task Valid_options_pass_validation()
  {
    var results = Validate(Options());

    await Assert.That(results.Count).IsEqualTo(0);
  }

  [Test]
  public async Task Non_http_endpoint_fails_validation()
  {
    var options = Options();
    options.WellKnownEndpoint = new Uri("about:blank");

    var results = Validate(options);

    await Assert.That(FailedMembers(results).Contains(nameof(OpenIdConnectHandlerOptions.WellKnownEndpoint))).IsTrue();
  }

  [Test]
  public async Task Empty_client_id_fails_validation()
  {
    var options = Options();
    options.ClientId = string.Empty;

    var results = Validate(options);

    await Assert.That(FailedMembers(results).Contains(nameof(OpenIdConnectHandlerOptions.ClientId))).IsTrue();
  }

  [Test]
  public async Task Whitespace_client_secret_fails_validation()
  {
    var options = Options();
    options.ClientSecret = "  ";

    var results = Validate(options);

    await Assert.That(FailedMembers(results).Contains(nameof(OpenIdConnectHandlerOptions.ClientSecret))).IsTrue();
  }

  private static OpenIdConnectHandlerOptions Options()
    => new()
    {
      WellKnownEndpoint = new Uri("https://issuer.example/.well-known/openid-configuration"),
      ClientId = "client",
      ClientSecret = "secret",
    };

  private static List<ValidationResult> Validate(OpenIdConnectHandlerOptions options)
  {
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
    return results;
  }

  private static IEnumerable<string> FailedMembers(IEnumerable<ValidationResult> results)
    => results.SelectMany(result => result.MemberNames);
}
