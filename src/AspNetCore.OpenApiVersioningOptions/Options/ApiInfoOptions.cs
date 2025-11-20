namespace Kritikos.AspNetCore.OpenApiVersioningOptions.Options;

using System.ComponentModel.DataAnnotations;

public abstract class OpenApiInfoOptions
{
  [Required(AllowEmptyStrings = false)]
  public string Title { get; set; } = string.Empty;

  [Required(AllowEmptyStrings = false)]
  public string Description { get; set; } = string.Empty;

  [Required(AllowEmptyStrings = false)]
  public string ContactName { get; set; } = string.Empty;

  [Required(AllowEmptyStrings = false)]
  [EmailAddress]
  public string ContactEmail { get; set; } = string.Empty;

  public string LicenseName { get; set; } = "Apache License, Version 2.0";

  public Uri LicenseUrl { get; set; } = new Uri("https://opensource.org/license/apache-2-0");
}
