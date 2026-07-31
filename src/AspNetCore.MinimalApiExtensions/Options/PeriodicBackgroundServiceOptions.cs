namespace Kritikos.AspNetCore.MinimalApiExtensions.Options;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Base configuration options for a periodic background service.
/// </summary>
public abstract class PeriodicBackgroundServiceOptions : IValidatableObject
{
  /// <summary>
  /// Gets or sets the interval between executions of the background service. Set to
  /// <see cref="Timeout.InfiniteTimeSpan"/> to run only when triggered.
  /// </summary>
  public TimeSpan Interval { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether a manual trigger should stop the currently executing work.
  /// </summary>
  public bool TriggerStopsCurrentExecution { get; set; }

  /// <inheritdoc />
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Interval <= TimeSpan.Zero && Interval != Timeout.InfiniteTimeSpan)
    {
      yield return new ValidationResult(
        "Interval must be greater than zero, or Timeout.InfiniteTimeSpan to run only when triggered.",
        [nameof(Interval)]);
    }
  }
}
