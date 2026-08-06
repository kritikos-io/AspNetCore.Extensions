namespace Kritikos.AspNetCore.VersioningOptions;

using System.Globalization;

using Asp.Versioning.ApiExplorer;

using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Options;

/// <summary>
/// An <see cref="IApiCatalogSource"/> that advertises each discovered API version's OpenAPI document in the
/// RFC 9727 api-catalog, so the catalog is populated from the API's versions rather than manual registration.
/// </summary>
/// <param name="versions">The provider describing the application's API versions.</param>
/// <param name="options">The catalog source options.</param>
public sealed class ApiVersionCatalogSource(
  IApiVersionDescriptionProvider versions,
  IOptions<ApiVersionCatalogOptions> options) : IApiCatalogSource
{
  private readonly ApiVersionCatalogOptions options = options.Value;

  /// <inheritdoc />
  public IEnumerable<ApiCatalogEntry> GetEntries(HttpContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var baseUri = $"{context.Request.Scheme}://{context.Request.Host}";

    return versions.ApiVersionDescriptions.Select(description =>
    {
      var document = new Uri(
        $"{baseUri}/{string.Format(CultureInfo.InvariantCulture, options.DocumentRoutePattern, description.GroupName)}");

      return new ApiCatalogEntry { Anchor = document, ServiceDescription = document };
    });
  }
}
