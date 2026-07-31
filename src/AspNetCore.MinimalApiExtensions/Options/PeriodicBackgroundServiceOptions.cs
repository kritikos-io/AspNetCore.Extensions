namespace Kritikos.AspNetCore.MinimalApiExtensions.Options;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Base configuration options for a periodic background service.
/// </summary>
public abstract class PeriodicBackgroundServiceOptions : IValidatableObject
{
  /// <summary>
  /// Gets or sets the interval between executions of the background service.
  /// </summary>
  public TimeSpan Interval { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether a manual trigger should stop the currently executing work.
  /// </summary>
  public bool TriggerStopsCurrentExecution { get; set; }

  /// <inheritdoc />
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Interval <= TimeSpan.Zero)
    {
      yield return new ValidationResult(
        "Interval must be greater than zero.",
        [nameof(Interval)]);
    }
  }
}
