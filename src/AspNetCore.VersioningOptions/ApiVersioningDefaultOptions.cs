namespace Kritikos.AspNetCore.VersioningOptions;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

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
  }

  /// <inheritdoc />
  public void Configure(ApiExplorerOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.GroupNameFormat = "'v'VV";
    options.SubstituteApiVersionInUrl = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
  }
}
