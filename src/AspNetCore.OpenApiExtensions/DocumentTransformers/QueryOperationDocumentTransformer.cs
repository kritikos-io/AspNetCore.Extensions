namespace Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi;

/// <summary>
/// Re-injects HTTP <c>QUERY</c> operations that the built-in OpenAPI generator drops (OpenAPI 3.1 has no
/// <c>query</c> path-item field, so the generator omits the operation while still emitting its request schema).
/// For each described QUERY endpoint, this adds an operation keyed by <see cref="HttpMethod.Query"/> carrying the
/// endpoint's request body, parameters, responses, and — for secured endpoints — 401/403 responses and a security
/// requirement (operation transformers cannot annotate these operations, which do not exist until this runs).
/// </summary>
/// <param name="securitySchemeId">The default security-scheme key for secured operations that declare no authentication scheme.</param>
/// <param name="schemesByAuthenticationScheme">
/// Map from ASP.NET Core authentication scheme name to OpenAPI security-scheme key, matching
/// <see cref="OperationTransformers.AuthorizationCheckOperationTransformer"/>.
/// </param>
public sealed partial class QueryOperationDocumentTransformer(
  string securitySchemeId = "bearer",
  IReadOnlyDictionary<string, string>? schemesByAuthenticationScheme = null)
  : IOpenApiDocumentTransformer
{
  private readonly IReadOnlyDictionary<string, string> schemesByAuthenticationScheme =
    schemesByAuthenticationScheme ?? new Dictionary<string, string>(StringComparer.Ordinal);

  /// <inheritdoc />
  public async Task TransformAsync(
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(document);
    ArgumentNullException.ThrowIfNull(context);

    foreach (var api in context.DescriptionGroups.SelectMany(static group => group.Items))
    {
      if (!HttpMethods.IsQuery(api.HttpMethod ?? string.Empty))
      {
        continue;
      }

      // Only inject into the document this description belongs to (mirrors OpenApiOptions.ShouldInclude's default),
      // otherwise a versioned QUERY endpoint would be added to every version's document.
      if (api.GroupName is not null && !string.Equals(api.GroupName, context.DocumentName, StringComparison.Ordinal))
      {
        continue;
      }

      document.Paths ??= [];
      var key = NormalizePath(api.RelativePath);

      OpenApiPathItem pathItem;
      if (document.Paths.TryGetValue(key, out var existing) && existing is OpenApiPathItem concrete)
      {
        pathItem = concrete;
      }
      else
      {
        pathItem = new OpenApiPathItem();
        document.Paths[key] = pathItem;
      }

      pathItem.Operations ??= [];
      if (pathItem.Operations.ContainsKey(HttpMethod.Query))
      {
        continue;
      }

      var operation = await BuildOperationAsync(api, document, context, cancellationToken);

      var metadata = api.ActionDescriptor.EndpointMetadata;
      if (OperationSecurity.IsSecured(metadata, out var authorization))
      {
        OperationSecurity.Apply(
          operation, document, metadata, authorization, securitySchemeId, this.schemesByAuthenticationScheme);
      }

      pathItem.Operations[HttpMethod.Query] = operation;
    }
  }

  /// <summary>Builds the OpenAPI path key for a description, stripping route constraints and trailing slashes.</summary>
  /// <param name="relativePath">The description's relative path (for example, <c>api/v1/pet/{id:long}</c>).</param>
  /// <returns>The normalized path key (for example, <c>/api/v1/pet/{id}</c>).</returns>
  internal static string NormalizePath(string? relativePath)
    => "/" + RouteParameterMatcher().Replace(relativePath ?? string.Empty, "{${name}}").Trim('/');

  /// <summary>
  /// Resolves the element type of an array or <see cref="IEnumerable{T}"/> worth emitting as an OpenAPI array,
  /// excluding <see cref="string"/>, byte sequences, and dictionaries (which serialize as strings or objects).
  /// </summary>
  /// <param name="type">The candidate type.</param>
  /// <param name="elementType">The resolved element type when the method returns <see langword="true"/>.</param>
  /// <returns><see langword="true"/> when <paramref name="type"/> is an array of <paramref name="elementType"/>.</returns>
  internal static bool TryGetElementType(Type type, [NotNullWhen(true)] out Type? elementType)
  {
    elementType = null;
    if (type == typeof(string))
    {
      return false;
    }

    if (type.IsArray)
    {
      elementType = type.GetElementType();
      return elementType is not null && elementType != typeof(byte);
    }

    Type[] interfaces = type.IsInterface ? [.. type.GetInterfaces(), type] : type.GetInterfaces();

    // Dictionaries implement IEnumerable<KeyValuePair<,>> but serialize as objects, not arrays.
    if (interfaces.Any(static i => i.IsGenericType
      && (i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
        || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))))
    {
      return false;
    }

    var enumerable = interfaces.FirstOrDefault(static i =>
      i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    if (enumerable is null)
    {
      return false;
    }

    elementType = enumerable.GetGenericArguments()[0];
    return elementType != typeof(byte) && elementType != typeof(char);
  }

  [GeneratedRegex(@"\{(?<name>[^:?=}]+)[^}]*\}", RegexOptions.CultureInvariant)]
  private static partial Regex RouteParameterMatcher();

  private static string? ResolveOperationId(ApiDescription api)
    => api.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault()?.Name;

  private static async Task<OpenApiOperation> BuildOperationAsync(
    ApiDescription api,
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    var operation = new OpenApiOperation { OperationId = ResolveOperationId(api) };

    var tags = api.ActionDescriptor.EndpointMetadata.OfType<ITagsMetadata>().FirstOrDefault()?.Tags;
    if (tags is { Count: > 0 })
    {
      operation.Tags = new HashSet<OpenApiTagReference>();
      foreach (var tag in tags)
      {
        operation.Tags.Add(new OpenApiTagReference(tag, document));
      }
    }

    foreach (var parameter in api.ParameterDescriptions)
    {
      var location = ResolveLocation(parameter.Source);
      if (location is null)
      {
        continue;
      }

      operation.Parameters ??= [];
      operation.Parameters.Add(new OpenApiParameter
      {
        Name = parameter.Name,
        In = location,
        Required = location == ParameterLocation.Path || parameter.IsRequired,
        Schema = await ResolveSchemaAsync(context, document, parameter.Type, parameter, cancellationToken),
      });
    }

    var body = api.ParameterDescriptions.FirstOrDefault(static p => p.Source == BindingSource.Body);
    if (body is not null)
    {
      var schema = await ResolveSchemaAsync(context, document, body.Type, body, cancellationToken);
      operation.RequestBody = new OpenApiRequestBody
      {
        Required = body.IsRequired,
        Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
        {
          ["application/json"] = new OpenApiMediaType { Schema = schema },
        },
      };
    }

    operation.Responses = await BuildResponsesAsync(api, document, context, cancellationToken);

    return operation;
  }

  private static async Task<OpenApiResponses> BuildResponsesAsync(
    ApiDescription api,
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    var responses = new OpenApiResponses();

    foreach (var responseType in api.SupportedResponseTypes)
    {
      var status = responseType.StatusCode.ToString(CultureInfo.InvariantCulture);
      var reason = ReasonPhrases.GetReasonPhrase(responseType.StatusCode);
      var response = new OpenApiResponse { Description = string.IsNullOrEmpty(reason) ? status : reason };

      if (responseType.Type is { } clrType && clrType != typeof(void) && responseType.ApiResponseFormats.Count > 0)
      {
        var schema = await ResolveSchemaAsync(context, document, clrType, null, cancellationToken);
        response.Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal);
        foreach (var format in responseType.ApiResponseFormats)
        {
          response.Content[format.MediaType] = new OpenApiMediaType { Schema = schema };
        }
      }

      responses[status] = response;
    }

    if (responses.Count == 0)
    {
      responses["200"] = new OpenApiResponse { Description = "OK" };
    }

    return responses;
  }

  private static ParameterLocation? ResolveLocation(BindingSource source)
    => source == BindingSource.Path ? ParameterLocation.Path
      : source == BindingSource.Query ? ParameterLocation.Query
        : source == BindingSource.Header ? ParameterLocation.Header
          : null;

  // Prefer a component reference over an inline schema to avoid duplicating a schema the generator already emitted.
  private static IOpenApiSchema ReferenceOrInline(IOpenApiSchema schema, Type type, OpenApiDocument document)
    => schema is OpenApiSchemaReference
      ? schema
      : document.Components?.Schemas is { } schemas && schemas.ContainsKey(type.Name)
        ? new OpenApiSchemaReference(type.Name, document)
        : schema;

  // Emit array schemas as { type: array, items: $ref } so element types reference their component instead of inlining.
  private static async Task<IOpenApiSchema> ResolveSchemaAsync(
    OpenApiDocumentTransformerContext context,
    OpenApiDocument document,
    Type type,
    ApiParameterDescription? parameter,
    CancellationToken cancellationToken)
  {
    if (TryGetElementType(type, out var elementType))
    {
      return new OpenApiSchema
      {
        Type = JsonSchemaType.Array,
        Items = await GenerateSchemaAsync(context, document, elementType, null, cancellationToken),
      };
    }

    return await GenerateSchemaAsync(context, document, type, parameter, cancellationToken);
  }

  // Build-time generation (dotnet-getdocument) does not register the keyed schema service GetOrCreateSchemaAsync
  // needs, so fall back to a component reference (or an unconstrained schema) when it is unavailable.
  private static async Task<IOpenApiSchema> GenerateSchemaAsync(
    OpenApiDocumentTransformerContext context,
    OpenApiDocument document,
    Type type,
    ApiParameterDescription? parameter,
    CancellationToken cancellationToken)
  {
    try
    {
      return ReferenceOrInline(
        await context.GetOrCreateSchemaAsync(type, parameter, cancellationToken), type, document);
    }
    catch (InvalidOperationException)
    {
      return document.Components?.Schemas is { } schemas && schemas.ContainsKey(type.Name)
        ? new OpenApiSchemaReference(type.Name, document)
        : new OpenApiSchema();
    }
  }
}
