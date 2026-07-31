namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.ComponentModel.DataAnnotations;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;

public sealed class CorrelationHeaderOptionsTests
{
  [Test]
  public async Task Default_header_passes_validation()
  {
    var results = Validate(new CorrelationHeaderOptions());

    await Assert.That(results.Count).IsEqualTo(0);
  }

  [Test]
  public async Task Empty_header_fails_validation()
  {
    var results = Validate(new CorrelationHeaderOptions { Header = string.Empty });

    await Assert.That(FailedMembers(results).Contains(nameof(CorrelationHeaderOptions.Header))).IsTrue();
  }

  [Test]
  public async Task Whitespace_header_fails_validation()
  {
    var results = Validate(new CorrelationHeaderOptions { Header = "   " });

    await Assert.That(FailedMembers(results).Contains(nameof(CorrelationHeaderOptions.Header))).IsTrue();
  }

  private static List<ValidationResult> Validate(CorrelationHeaderOptions options)
  {
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
    return results;
  }

  private static IEnumerable<string> FailedMembers(IEnumerable<ValidationResult> results)
    => results.SelectMany(result => result.MemberNames);
}
