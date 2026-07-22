namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.ComponentModel.DataAnnotations;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;

public sealed class HeartbeatOptionsTests
{
  [Test]
  public async Task Defaults_pass_validation()
  {
    var results = Validate(new HeartbeatOptions());

    await Assert.That(results.Count).IsEqualTo(0);
  }

  [Test]
  public async Task DefaultTimeout_of_zero_fails_validation()
  {
    var results = Validate(new HeartbeatOptions { DefaultTimeout = TimeSpan.Zero });

    await Assert.That(FailedMembers(results).Contains(nameof(HeartbeatOptions.DefaultTimeout))).IsTrue();
  }

  [Test]
  public async Task DefaultTimeout_that_is_negative_fails_validation()
  {
    var results = Validate(new HeartbeatOptions { DefaultTimeout = TimeSpan.FromSeconds(-1) });

    await Assert.That(FailedMembers(results).Contains(nameof(HeartbeatOptions.DefaultTimeout))).IsTrue();
  }

  [Test]
  public async Task WatchdogInterval_of_zero_fails_validation()
  {
    var results = Validate(new HeartbeatOptions { WatchdogInterval = TimeSpan.Zero });

    await Assert.That(FailedMembers(results).Contains(nameof(HeartbeatOptions.WatchdogInterval))).IsTrue();
  }

  [Test]
  public async Task WatchdogInterval_that_is_negative_fails_validation()
  {
    var results = Validate(new HeartbeatOptions { WatchdogInterval = TimeSpan.FromSeconds(-1) });

    await Assert.That(FailedMembers(results).Contains(nameof(HeartbeatOptions.WatchdogInterval))).IsTrue();
  }

  private static List<ValidationResult> Validate(HeartbeatOptions options)
  {
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
    return results;
  }

  private static IEnumerable<string> FailedMembers(IEnumerable<ValidationResult> results)
    => results.SelectMany(result => result.MemberNames);
}
