namespace Kritikos.PetStore.WebApi;

using Asp.Versioning;

using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.MinimalApiExtensions.StartupBuilder;
using Kritikos.AspNetCore.OpenApiExtensions.OperationTransformers;
using Kritikos.AspNetCore.OpenApiExtensions.SchemaTransformers;
using Kritikos.AspNetCore.OpenApiFeatureManagementOptions;
using Kritikos.AspNetCore.OpenApiOidcExtensions.DocumentTransformers;
using Kritikos.AspNetCore.OpenApiVersioningOptions;
using Kritikos.AspNetCore.OpenApiVersioningOptions.DocumentTransformers;
using Kritikos.AspNetCore.VersioningOptions;
using Kritikos.Extensions.Options.DependencyInjection;

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
    builder.Services.AddValidation();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddMvcCore();

    builder.Services.AddCorrelationHeader();

    builder.Services.AddProblemDetails();

    builder.Services.AddFeatureManagement();
    builder.Services.AddHttpClient();

    builder.Services.AddOptionsDefinition<MyOpenApiInfoOptions>();
    builder.Services.AddOptionsDefinition<MyOpenApiOpenIdOptions>();

    builder.Services.AddVersionedOpenApi<Program>(static options =>
    {
      options.AddSchemaTransformer<NullableSchemaTransformer>();
      options.AddOperationTransformer<AuthorizationCheckOperationTransformer>();

      options.AddDocumentTransformer<ApiVersionDocumentTransformer<MyOpenApiInfoOptions>>();
      options.AddDocumentTransformer<FeatureFilterDocumentTransformer>();

      options.AddDocumentTransformer<OidcSecuritySchemeTransformer<MyOpenApiOpenIdOptions>>();
    });

    builder.Services.AddApiVersioningDefaults();

    builder.Services.AddApiVersionModelProvider<Program>();
    builder.Services.AddApiVersionSetProvider<Program>();
  }

  /// <inheritdoc />
  public void Configure(WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);

    app.AddApiVersionSet<Program>(static options => options
      .ReportApiVersions());

    app.UseHttpsRedirection();

    app.UseRouting();

    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
      var options = app.Services.GetRequiredService<IOptions<MyOpenApiOpenIdOptions>>().Value;

      o.AddDocuments(Program.VersionModel.ImplementedApiVersions.Select(static v => $"v{v}"));
      o.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
      o.AddPreferredSecuritySchemes("oidc");
      o.AddAuthorizationCodeFlow("oidc", flow =>
      {
        flow.WithClientId(options.ClientId);
        flow.SelectedScopes = ["openid", "profile", "email"];
        flow.Pkce = Pkce.Sha256;
      });
    });

    var neutral = app.NewApiVersionSet("neutral")
      .IsApiVersionNeutral()
      .Build();
    app
      .MapGroup("api")
      .WithApiVersionSet(neutral)
      .IsApiVersionNeutral()
      .MapGet("versions", static (ApiVersionModel versions) =>
      {
        var result = new VersionDto(
          [.. versions.SupportedApiVersions.Select(static x => $"v{x}")],
          [.. versions.DeprecatedApiVersions.Select(static x => $"v{x}")]);
        return TypedResults.Ok(result);
      });

    app.UseCorrelationHeader();
    app.MapEndpoints();
  }
}
