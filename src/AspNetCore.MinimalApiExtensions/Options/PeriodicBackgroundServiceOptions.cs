namespace Kritikos.AspNetCore.MinimalApiExtensions.Options;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Base configuration options for a periodic background service.
/// </summary>
public abstract class PeriodicBackgroundServiceOptions
{
  /// <summary>
  /// Gets or sets the interval between executions of the background service.
  /// </summary>
  [Required]
  public TimeSpan Interval { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether a manual trigger should stop the currently executing work.
  /// </summary>
  public bool TriggerStopsCurrentExecution { get; set; }
}
