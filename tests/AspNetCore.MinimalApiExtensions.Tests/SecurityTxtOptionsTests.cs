namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.ComponentModel.DataAnnotations;

using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

public sealed class SecurityTxtOptionsTests
{
  [Test]
  public async Task Valid_options_pass_validation()
  {
    var options = new SecurityTxtOptions { Expires = DateTimeOffset.UtcNow };
    options.Contact.Add(new Uri("mailto:security@example.com"));

    await Assert.That(Validate(options).Count).IsEqualTo(0);
  }

  [Test]
  public async Task Missing_contact_fails_validation()
  {
    var options = new SecurityTxtOptions { Expires = DateTimeOffset.UtcNow };

    await Assert.That(FailedMembers(Validate(options)).Contains(nameof(SecurityTxtOptions.Contact))).IsTrue();
  }

  [Test]
  public async Task Missing_expires_fails_validation()
  {
    var options = new SecurityTxtOptions();
    options.Contact.Add(new Uri("mailto:security@example.com"));

    await Assert.That(FailedMembers(Validate(options)).Contains(nameof(SecurityTxtOptions.Expires))).IsTrue();
  }

  [Test]
  public async Task Http_contact_fails_validation()
  {
    var options = new SecurityTxtOptions { Expires = DateTimeOffset.UtcNow };
    options.Contact.Add(new Uri("http://example.com/insecure"));

    await Assert.That(FailedMembers(Validate(options)).Contains(nameof(SecurityTxtOptions.Contact))).IsTrue();
  }

  private static List<ValidationResult> Validate(SecurityTxtOptions options)
  {
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
    return results;
  }

  private static IEnumerable<string> FailedMembers(IEnumerable<ValidationResult> results)
    => results.SelectMany(result => result.MemberNames);
}
