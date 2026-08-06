namespace Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Microsoft.Extensions.Options;

/// <summary>
/// Maps the RFC 8615 well-known endpoints (security.txt, oauth-protected-resource, and api-catalog).
/// </summary>
public static class KritikosWellKnownEndpointRouteBuilderExtensions
{
  private const string ApiCatalogContentType =
    "application/linkset+json; profile=\"https://www.rfc-editor.org/info/rfc9727\"";

  /// <summary>
  /// Maps the RFC 9116 <c>/.well-known/security.txt</c> endpoint from the configured <see cref="SecurityTxtOptions"/>.
  /// </summary>
  /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to map onto.</param>
  /// <returns>A <see cref="RouteHandlerBuilder"/> for further configuration (for example, authorization).</returns>
  /// <exception cref="ArgumentNullException"><paramref name="endpoints"/> is <see langword="null"/>.</exception>
  public static RouteHandlerBuilder MapSecurityTxt(this IEndpointRouteBuilder endpoints)
  {
    ArgumentNullException.ThrowIfNull(endpoints);

    return endpoints
      .MapGet(
        "/.well-known/security.txt",
        static (IOptions<SecurityTxtOptions> options) =>
          TypedResults.Text(CreateSecurityTxt(options.Value), "text/plain; charset=utf-8"))
      .ExcludeFromDescription();
  }

  /// <summary>
  /// Maps the RFC 9728 <c>/.well-known/oauth-protected-resource</c> endpoint from the configured
  /// <see cref="OAuthProtectedResourceOptions"/>.
  /// </summary>
  /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to map onto.</param>
  /// <returns>A <see cref="RouteHandlerBuilder"/> for further configuration (for example, authorization).</returns>
  /// <exception cref="ArgumentNullException"><paramref name="endpoints"/> is <see langword="null"/>.</exception>
  public static RouteHandlerBuilder MapOAuthProtectedResource(this IEndpointRouteBuilder endpoints)
  {
    ArgumentNullException.ThrowIfNull(endpoints);

    return endpoints
      .MapGet(
        "/.well-known/oauth-protected-resource",
        static (IOptions<OAuthProtectedResourceOptions> options, HttpContext context) =>
        {
          var resource = options.Value.Resource
            ?? new Uri($"{context.Request.Scheme}://{context.Request.Host}");
          return TypedResults.Text(
            CreateOAuthProtectedResource(options.Value, resource).ToJsonString(),
            "application/json");
        })
      .ExcludeFromDescription();
  }

  /// <summary>
  /// Maps the RFC 9727 <c>/.well-known/api-catalog</c> endpoint, composed from the registered
  /// <see cref="IApiCatalogSource"/> contributors.
  /// </summary>
  /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to map onto.</param>
  /// <returns>A <see cref="RouteHandlerBuilder"/> for further configuration (for example, authorization).</returns>
  /// <exception cref="ArgumentNullException"><paramref name="endpoints"/> is <see langword="null"/>.</exception>
  public static RouteHandlerBuilder MapApiCatalog(this IEndpointRouteBuilder endpoints)
  {
    ArgumentNullException.ThrowIfNull(endpoints);

    return endpoints
      .MapMethods(
        "/.well-known/api-catalog",
        ["GET", "HEAD"],
        static (IEnumerable<IApiCatalogSource> sources, HttpContext context) =>
        {
          context.Response.Headers.Link = $"<{context.Request.Path}>; rel=\"api-catalog\"";
          var entries = sources.SelectMany(source => source.GetEntries(context));
          return TypedResults.Text(CreateApiCatalog(entries).ToJsonString(), ApiCatalogContentType);
        })
      .ExcludeFromDescription();
  }

  /// <summary>Builds the RFC 9116 <c>security.txt</c> document body from the options.</summary>
  /// <param name="options">The security.txt options.</param>
  /// <returns>The document body.</returns>
  internal static string CreateSecurityTxt(SecurityTxtOptions options)
  {
    var builder = new StringBuilder();

    AppendUris(builder, "Contact", options.Contact);

    if (options.Expires is { } expires)
    {
      builder
        .Append("Expires: ")
        .Append(expires.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture))
        .Append('\n');
    }

    AppendUris(builder, "Encryption", options.Encryption);
    AppendUris(builder, "Acknowledgments", options.Acknowledgments);
    AppendUris(builder, "Policy", options.Policy);
    AppendUris(builder, "Hiring", options.Hiring);
    AppendUris(builder, "Canonical", options.Canonical);

    if (options.PreferredLanguages.Count > 0)
    {
      builder.Append("Preferred-Languages: ").Append(string.Join(", ", options.PreferredLanguages)).Append('\n');
    }

    return builder.ToString();
  }

  /// <summary>Builds the RFC 9728 protected-resource metadata document from the options.</summary>
  /// <param name="options">The protected-resource options.</param>
  /// <param name="resource">The resolved resource identifier (the configured value, or the request base URL).</param>
  /// <returns>The metadata document.</returns>
  internal static JsonObject CreateOAuthProtectedResource(OAuthProtectedResourceOptions options, Uri resource)
  {
    var json = new JsonObject { ["resource"] = resource.OriginalString };

    AddArray(json, "authorization_servers", options.AuthorizationServers, static uri => uri.OriginalString);
    AddArray(json, "scopes_supported", options.ScopesSupported, static value => value);
    AddArray(json, "bearer_methods_supported", options.BearerMethodsSupported, static value => value);

    if (options.JwksUri is not null)
    {
      json["jwks_uri"] = options.JwksUri.OriginalString;
    }

    if (options.ResourceDocumentation is not null)
    {
      json["resource_documentation"] = options.ResourceDocumentation.OriginalString;
    }

    if (!string.IsNullOrWhiteSpace(options.ResourceName))
    {
      json["resource_name"] = options.ResourceName;
    }

    return json;
  }

  /// <summary>Builds the RFC 9727 api-catalog linkset document from the entries.</summary>
  /// <param name="apis">The catalog entries (manual options merged with contributed sources).</param>
  /// <returns>The linkset document.</returns>
  internal static JsonObject CreateApiCatalog(IEnumerable<ApiCatalogEntry> apis)
  {
    var linkset = new JsonArray();

    foreach (var api in apis)
    {
      var entry = new JsonObject
      {
        ["anchor"] = api.Anchor?.OriginalString,
        ["service-desc"] = new JsonArray(new JsonObject
        {
          ["href"] = api.ServiceDescription?.OriginalString,
          ["type"] = api.ServiceDescriptionType,
        }),
      };

      if (api.Documentation is not null)
      {
        entry["service-doc"] = new JsonArray(new JsonObject
        {
          ["href"] = api.Documentation.OriginalString,
          ["type"] = api.DocumentationType,
        });
      }

      linkset.Add(entry);
    }

    return new JsonObject { ["linkset"] = linkset };
  }

  private static void AppendUris(StringBuilder builder, string field, IList<Uri> uris)
  {
    foreach (var uri in uris)
    {
      builder.Append(field).Append(": ").Append(uri.OriginalString).Append('\n');
    }
  }

  private static void AddArray<T>(JsonObject json, string name, IList<T> values, Func<T, string> selector)
  {
    if (values.Count == 0)
    {
      return;
    }

    var array = new JsonArray();
    foreach (var value in values)
    {
      array.Add(selector(value));
    }

    json[name] = array;
  }
}
