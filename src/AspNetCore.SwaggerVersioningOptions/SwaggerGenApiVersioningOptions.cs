namespace Kritikos.AspNetCore.SwaggerVersioningOptions;

using System.Text;

using Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerGenApiVersioningOptions
    : IConfigureOptions<SwaggerGenOptions>
{
  private readonly IApiVersionDescriptionProvider provider;
  private readonly OpenApiInfo apiInfo;

  public SwaggerGenApiVersioningOptions(IApiVersionDescriptionProvider provider, OpenApiInfo apiInfo)
  {
    this.apiInfo = apiInfo;
    this.provider = provider;
  }

  /// <inheritdoc />
  public void Configure(SwaggerGenOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.OperationFilter<SwaggerDefaultVersioningValues>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
      options.SwaggerDoc($"{description.GroupName}", CreateInfoForApiVersion(description));
    }
  }

  private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
  {
    var text = new StringBuilder(apiInfo.Description);
    var info = new OpenApiInfo
    {
      Title = apiInfo.Title, Contact = apiInfo.Contact, License = apiInfo.License, Version = description.ApiVersion.ToString(),
    };
    if (description.IsDeprecated)
    {
      text.Append(" This Api Version has been deprecated.");
    }

    if (description.SunsetPolicy is { } policy)
    {
      if (policy.Date is { } when)
      {
        text.Append(" This Api Version will be sunset on ")
            .Append(when.Date.ToShortDateString());
      }

      if (policy.HasLinks)
      {
        text.AppendLine();
        var rendered = false;
        foreach (var link in policy.Links)
        {
          if (link.Type != "text/html")
          {
            continue;
          }

          if (!rendered)
          {
            text.Append("<h4>Links</h4><ul>");
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
          text.Append("</ul>");
        }
      }
    }

    text.Append("<h4>Additional Information</h4>");
    info.Description = text.ToString();
    return info;
  }
}
