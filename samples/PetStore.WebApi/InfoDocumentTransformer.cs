namespace Kritikos.PetStore.WebApi;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

public sealed class InfoDocumentTransformer(IOptions<MyOpenApiInfoOptions> options)
  : IOpenApiDocumentTransformer
{
  private readonly MyOpenApiInfoOptions options = options.Value;

  public Task TransformAsync(
    OpenApiDocument document,
    OpenApiDocumentTransformerContext context,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(document);

    document.Info.Title = options.Title;
    document.Info.Contact = new OpenApiContact { Name = options.ContactName, Email = options.ContactEmail };
    document.Info.License = new OpenApiLicense { Name = options.LicenseName, Url = options.LicenseUrl };

    return Task.CompletedTask;
  }
}
