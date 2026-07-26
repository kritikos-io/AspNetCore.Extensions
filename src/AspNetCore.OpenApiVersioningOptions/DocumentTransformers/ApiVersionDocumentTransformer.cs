namespace Kritikos.AspNetCore.OpenApiVersioningOptions.DocumentTransformers;

using System.Text;

using Asp.Versioning.ApiExplorer;

using Kritikos.AspNetCore.OpenApiVersioningOptions.Options;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi;

/// <summary>
/// An OpenAPI document transformer that populates document info (title, description, contact, license) from versioned API metadata.
/// </summary>
/// <typeparam name="TApiInfoOptions">The options type containing API info metadata.</typeparam>
/// <param name="options">The API info options providing title, description, contact, and license data.</param>
/// <param name="versionProvider">The API version description provider.</param>
[Obsolete(
  "Superseded by the Asp.Versioning.OpenApi package (VersionedOpenApiOptions.DocumentDescription: "
  + "DeprecationNotice/SunsetNotice/HidePolicyLinks). This API will be removed in a future release.")]
public sealed class ApiVersionDocumentTransformer<TApiInfoOptions>(
  IOptions<TApiInfoOptions> options,
  IApiVersionDescriptionProvider versionProvider)
  : IOpenApiDocumentTransformer
  where TApiInfoOptions : OpenApiInfoOptions
{
  private readonly IApiVersionDescriptionProvider versionProvider = versionProvider;
  private readonly TApiInfoOptions options = options.Value;

  /// <inheritdoc />
  public Task TransformAsync(
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(document);

    var apiDescription = versionProvider.ApiVersionDescriptions
      .SingleOrDefault(x => x.GroupName == context.DocumentName);
    if (apiDescription == null)
    {
      return Task.CompletedTask;
    }

    document.Info.Version = apiDescription.ApiVersion.ToString();
    document.Info.Title = options.Title;
    document.Info.Description = BuildDescription(apiDescription, options.Description);

    document.Info.License = new OpenApiLicense { Name = options.LicenseName, Url = options.LicenseUrl, };

    document.Info.Contact = new OpenApiContact { Name = options.ContactName, Email = options.ContactEmail, };
    return Task.CompletedTask;
  }

  private static string BuildDescription(ApiVersionDescription api, string description)
  {
    var text = new StringBuilder(description);
    if (api.IsDeprecated)
    {
      if (text.Length > 0)
      {
        if (text[^1] != '.')
        {
          text.AppendLine();
        }

        text.Append(' ');
      }

      text.Append("This API version has been deprecated.");
    }

    if (api.SunsetPolicy is not { } policy)
    {
      return text.ToString();
    }

    if (policy.Date is { } when)
    {
      if (text.Length > 0)
      {
        text.Append(' ');
      }

      text.Append("This API version will be sunset on ")
        .Append(when.Date.ToShortDateString())
        .Append('.');
    }

    if (policy.HasLinks)
    {
      text.AppendLine();

      var rendered = false;
      foreach (var link in policy.Links.Where(static l => l.Type == "text/html"))
      {
        if (!rendered)
        {
          text.AppendLine("For more information, please visit:");
          rendered = true;
        }

        text.Append("<li><a href=\"");
        text.Append(link.LinkTarget.OriginalString);
        text.Append("\">");
        text.Append(
          StringSegment.IsNullOrEmpty(link.Title)
            ? link.LinkTarget.OriginalString
            : link.Title.ToString());
        text.Append("</a></li>");
      }

      if (rendered)
      {
        text.AppendLine("</ul>");
      }
    }

    return text.ToString();
  }
}
