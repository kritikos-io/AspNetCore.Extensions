namespace Kritikos.AspNetCore.SwaggerFeatureManagementOptions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerFeatureOptions : IConfigureOptions<SwaggerGenOptions>
{
  /// <inheritdoc />
  public void Configure(SwaggerGenOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.DocumentFilter<SwaggerFeatureGateFilter>();
  }
}
