namespace Kritikos.AspNetCore.VersioningOptions;

/// <summary>
/// Options controlling how <see cref="ApiVersionCatalogSource"/> builds OpenAPI document links.
/// </summary>
public sealed class ApiVersionCatalogOptions
{
  /// <summary>
  /// Gets or sets the composite format used to build each version's OpenAPI document path from its group name.
  /// Defaults to <c>openapi/{0}.json</c> (the standard <c>MapOpenApi</c> route).
  /// </summary>
  public string DocumentRoutePattern { get; set; } = "openapi/{0}.json";
}
