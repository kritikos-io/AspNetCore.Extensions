namespace Kritikos.AspNetCore.OpenApiVersioningOptions.Options;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Base configuration options for OpenAPI document info (title, description, contact, license).
/// </summary>
public abstract class OpenApiInfoOptions
{
  /// <summary>
  /// Gets or sets the API title.
  /// </summary>
  [Required(AllowEmptyStrings = false)]
  public string Title { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the API description.
  /// </summary>
  [Required(AllowEmptyStrings = false)]
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the contact name for the API.
  /// </summary>
  [Required(AllowEmptyStrings = false)]
  public string ContactName { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the contact email address for the API.
  /// </summary>
  [Required(AllowEmptyStrings = false)]
  [EmailAddress]
  public string ContactEmail { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the license name for the API.
  /// </summary>
  public string LicenseName { get; set; } = "Apache License, Version 2.0";

  /// <summary>
  /// Gets or sets the license URL for the API.
  /// </summary>
  public Uri LicenseUrl { get; set; } = new Uri("https://opensource.org/license/apache-2-0");
}
