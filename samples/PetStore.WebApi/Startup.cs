namespace Kritikos.PetStore.WebApi;

using System.Security.Cryptography.X509Certificates;

using Asp.Versioning;

using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.MinimalApiExtensions.StartupBuilder;
using Kritikos.AspNetCore.OpenApiExtensions.OperationTransformers;
using Kritikos.AspNetCore.OpenApiExtensions.SchemaTransformers;
using Kritikos.AspNetCore.OpenApiFeatureManagementOptions;
using Kritikos.AspNetCore.OpenApiOidcExtensions.DocumentTransformers;
using Kritikos.AspNetCore.OpenApiVersioningOptions.DocumentTransformers;
using Kritikos.AspNetCore.VersioningOptions;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

using Scalar.AspNetCore;

public class Startup : IWebApplicationStartup
{
  /// <inheritdoc />
  public void ConfigureServices(WebApplicationBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.Services.AddEndpoints(typeof(Startup));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddMvcCore();

    builder.Services.AddCorrelationHeader();

    builder.Services.AddProblemDetails();

    builder.Services.AddFeatureManagement();
    builder.Services.AddHttpClient();

    builder.Services.AddOptionsDefinition<MyOpenApiInfoOptions>();
    builder.Services.AddOptionsDefinition<MyOpenApiOpenIdOptions>();

    builder.Services.AddOpenApi("v1",
        options =>
        {
          options.AddDocumentTransformer<ApiVersionDocumentTransformer<MyOpenApiInfoOptions>>();
          options.AddOperationTransformer<AuthorizationCheckOperationTransformer>();
          options.AddSchemaTransformer<NullableSchemaTransformer>();
          options.AddDocumentTransformer<FeatureFilterDocumentTransformer>();
          options.AddDocumentTransformer<OidcSecuritySchemeTransformer<MyOpenApiOpenIdOptions>>();
        });
    builder.Services.AddOpenApi("v2",
        options =>
        {
          options.AddDocumentTransformer<ApiVersionDocumentTransformer<MyOpenApiInfoOptions>>();
          options.AddOperationTransformer<AuthorizationCheckOperationTransformer>();
          options.AddSchemaTransformer<NullableSchemaTransformer>();
          options.AddDocumentTransformer<FeatureFilterDocumentTransformer>();
          options.AddDocumentTransformer<OidcSecuritySchemeTransformer<MyOpenApiOpenIdOptions>>();
        });
    builder.Services.AddOpenApi("v3",
        options =>
        {
          options.AddDocumentTransformer<ApiVersionDocumentTransformer<MyOpenApiInfoOptions>>();
          options.AddOperationTransformer<AuthorizationCheckOperationTransformer>();
          options.AddSchemaTransformer<NullableSchemaTransformer>();
          options.AddDocumentTransformer<FeatureFilterDocumentTransformer>();
          options.AddDocumentTransformer<OidcSecuritySchemeTransformer<MyOpenApiOpenIdOptions>>();
        });

    builder.Services.AddApiVersioningDefaults();
  }

  /// <inheritdoc />
  public void Configure(WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);
    Program.VersionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .HasApiVersion(new ApiVersion(2))
        .HasApiVersion(new ApiVersion(3))
        .HasDeprecatedApiVersion(new ApiVersion(0))
        .ReportApiVersions()
        .Build();

    app.UseHttpsRedirection();

    app.UseRouting();

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCorrelationHeader();
    app.MapEndpoints();
  }
}
