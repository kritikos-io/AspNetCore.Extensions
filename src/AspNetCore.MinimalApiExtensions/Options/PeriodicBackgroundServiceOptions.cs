namespace Kritikos.AspNetCore.MinimalApiExtensions.Options;

using System.ComponentModel.DataAnnotations;

public abstract class PeriodicBackgroundServiceOptions<T>
{
  [Required]
  public TimeSpan Interval { get; set; }
}
