namespace Kritikos.AspNetCore.MinimalApiExtensions.Options;

using System.ComponentModel.DataAnnotations;

public abstract class PeriodicBackgroundServiceOptions
{
  [Required]
  public TimeSpan Interval { get; set; }

  public bool TriggerStopsCurrentExecution { get; set; }
}
