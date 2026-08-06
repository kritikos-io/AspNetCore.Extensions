namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.ComponentModel.DataAnnotations;

using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

public sealed class OAuthProtectedResourceOptionsTests
{
  [Test]
  public async Task Valid_options_pass_validation()
  {
    var options = new OAuthProtectedResourceOptions { Resource = new Uri("https://resource.example.com") };
    options.BearerMethodsSupported.Add("header");

    await Assert.That(Validate(options).Count).IsEqualTo(0);
  }

  [Test]
  public async Task Missing_resource_passes_validation()
  {
    var options = new OAuthProtectedResourceOptions();

    await Assert.That(Validate(options).Count).IsEqualTo(0);
  }

  [Test]
  public async Task Http_resource_fails_validation()
  {
    var options = new OAuthProtectedResourceOptions { Resource = new Uri("http://resource.example.com") };

    await Assert.That(FailedMembers(Validate(options)).Contains(nameof(OAuthProtectedResourceOptions.Resource))).IsTrue();
  }

  [Test]
  public async Task Resource_with_fragment_fails_validation()
  {
    var options = new OAuthProtectedResourceOptions { Resource = new Uri("https://resource.example.com/#frag") };

    await Assert.That(FailedMembers(Validate(options)).Contains(nameof(OAuthProtectedResourceOptions.Resource))).IsTrue();
  }

  [Test]
  public async Task Unknown_bearer_method_fails_validation()
  {
    var options = new OAuthProtectedResourceOptions { Resource = new Uri("https://resource.example.com") };
    options.BearerMethodsSupported.Add("cookie");

    await Assert.That(FailedMembers(Validate(options)).Contains(nameof(OAuthProtectedResourceOptions.BearerMethodsSupported))).IsTrue();
  }

  private static List<ValidationResult> Validate(OAuthProtectedResourceOptions options)
  {
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
    return results;
  }

  private static IEnumerable<string> FailedMembers(IEnumerable<ValidationResult> results)
    => results.SelectMany(result => result.MemberNames);
}
