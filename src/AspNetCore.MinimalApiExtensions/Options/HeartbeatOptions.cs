namespace Kritikos.AspNetCore.MinimalApiExtensions.Options;

using System.ComponentModel.DataAnnotations;

using Kritikos.Extensions.Options.Contracts;

/// <summary>Configuration for the heartbeat health check and watchdog.</summary>
public sealed class HeartbeatOptions : IOptionsDefinition, IValidatableObject
{
  /// <inheritdoc />
  public static string Location { get; } = "AspNetCore:Heartbeat";

  /// <summary>
  /// Gets or sets the default staleness window applied to heartbeat sources that register without their own timeout.
  /// </summary>
  public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(15);

  /// <summary>
  /// Gets or sets how often the watchdog scans for stale sources to cancel. Smaller values lower cancellation
  /// latency at the cost of more frequent scans.
  /// </summary>
  public TimeSpan WatchdogInterval { get; set; } = TimeSpan.FromSeconds(10);

  /// <inheritdoc />
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (DefaultTimeout <= TimeSpan.Zero)
    {
      yield return new ValidationResult(
        "DefaultTimeout must be greater than zero.",
        [nameof(DefaultTimeout)]);
    }

    if (WatchdogInterval <= TimeSpan.Zero)
    {
      yield return new ValidationResult(
        "WatchdogInterval must be greater than zero.",
        [nameof(WatchdogInterval)]);
    }
  }
}
