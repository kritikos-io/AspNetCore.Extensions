namespace Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

/// <summary>
/// A single API entry within the RFC 9727 api-catalog document.
/// </summary>
public sealed class ApiCatalogEntry
{
  /// <summary>Gets or sets the anchor URI identifying the API (its <c>service-desc</c> link context).</summary>
  public Uri? Anchor { get; set; }

  /// <summary>Gets or sets the machine-readable API description URI (for example, the OpenAPI document).</summary>
  public Uri? ServiceDescription { get; set; }

  /// <summary>Gets or sets the media type of the <see cref="ServiceDescription"/> link.</summary>
  public string ServiceDescriptionType { get; set; } = "application/openapi+json";

  /// <summary>Gets or sets the human-readable documentation URI for the API.</summary>
  public Uri? Documentation { get; set; }

  /// <summary>Gets or sets the media type of the <see cref="Documentation"/> link.</summary>
  public string DocumentationType { get; set; } = "text/html";
}
