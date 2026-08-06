namespace Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Contributes entries to the RFC 9727 api-catalog document, letting higher-level libraries
/// (for example, API versioning or OpenAPI) auto-populate the catalog instead of manual registration.
/// </summary>
public interface IApiCatalogSource
{
  /// <summary>Produces catalog entries for the current request.</summary>
  /// <param name="context">The current <see cref="HttpContext"/>, used to build absolute links.</param>
  /// <returns>The contributed <see cref="ApiCatalogEntry"/> items.</returns>
  IEnumerable<ApiCatalogEntry> GetEntries(HttpContext context);
}
