namespace Kritikos.AspNetCore.VersioningOptions;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

public class ApiVersioningDefaultOptions
  : IConfigureOptions<ApiVersioningOptions>,
    IConfigureOptions<ApiExplorerOptions>
{
  /// <inheritdoc />
  public void Configure(ApiVersioningOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.UnsupportedApiVersionStatusCode = StatusCodes.Status503ServiceUnavailable;
  }

  /// <inheritdoc />
  public void Configure(ApiExplorerOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.AssumeDefaultVersionWhenUnspecified = true;
    options.SubstituteApiVersionInUrl = true;
    options.RouteConstraintName = "apiVersion";
    options.GroupNameFormat = "'v'V";
  }
}
