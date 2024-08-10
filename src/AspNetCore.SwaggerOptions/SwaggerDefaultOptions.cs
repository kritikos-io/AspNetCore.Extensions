namespace Kritikos.AspNetCore.SwaggerOptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

internal sealed class SwaggerDefaultOptions
  : IConfigureOptions<SwaggerOptions>,
    IConfigureOptions<SwaggerGenOptions>,
    IConfigureOptions<SwaggerUIOptions>
{
  /// <inheritdoc />
  public void Configure(SwaggerOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.PreSerializeFilters.Add((openApi, request) => openApi.Servers =
      [new OpenApiServer { Url = $"{request.Scheme}://{request.Host.Value}", }]);
  }

  /// <inheritdoc />
  public void Configure(SwaggerGenOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.EnableAnnotations(true, true);
    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
    options.OperationFilter<SecurityRequirementsOperationFilter>();

    options.InferSecuritySchemes();
    options.DescribeAllParametersInCamelCase();

    options.UseAllOfForInheritance();
    options.UseOneOfForPolymorphism();
  }

  /// <inheritdoc />
  public void Configure(SwaggerUIOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.DocExpansion(DocExpansion.None);

    options.DisplayOperationId();
    options.DisplayRequestDuration();

    options.EnableDeepLinking();
    options.EnableFilter();
    options.EnablePersistAuthorization();
    options.EnableTryItOutByDefault();
    options.EnableValidator();

    options.ShowExtensions();
  }
}
