namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.ComponentModel.DataAnnotations;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;

public sealed class PeriodicBackgroundServiceOptionsTests
{
  [Test]
  public async Task A_positive_interval_passes_validation()
  {
    var results = Validate(new TestOptions { Interval = TimeSpan.FromSeconds(1) });

    await Assert.That(results.Count).IsEqualTo(0);
  }

  [Test]
  public async Task An_infinite_interval_passes_validation()
  {
    var results = Validate(new TestOptions { Interval = Timeout.InfiniteTimeSpan });

    await Assert.That(results.Count).IsEqualTo(0);
  }

  [Test]
  public async Task Interval_of_zero_fails_validation()
  {
    var results = Validate(new TestOptions { Interval = TimeSpan.Zero });

    await Assert.That(FailedMembers(results).Contains(nameof(PeriodicBackgroundServiceOptions.Interval))).IsTrue();
  }

  [Test]
  public async Task Interval_that_is_negative_fails_validation()
  {
    var results = Validate(new TestOptions { Interval = TimeSpan.FromSeconds(-1) });

    await Assert.That(FailedMembers(results).Contains(nameof(PeriodicBackgroundServiceOptions.Interval))).IsTrue();
  }

  private static List<ValidationResult> Validate(PeriodicBackgroundServiceOptions options)
  {
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
    return results;
  }

  private static IEnumerable<string> FailedMembers(IEnumerable<ValidationResult> results)
    => results.SelectMany(result => result.MemberNames);

  private sealed class TestOptions : PeriodicBackgroundServiceOptions;
}
