namespace Kritikos.PetStore.WebApi;

using Asp.Versioning;

using Kritikos.AspNetCore.MinimalApiExtensions.Authentication;
using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.MinimalApiExtensions.StartupBuilder;
using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;
using Kritikos.AspNetCore.OpenApiExtensions;
using Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;
using Kritikos.AspNetCore.OpenApiExtensions.OperationTransformers;
using Kritikos.AspNetCore.OpenApiFeatureManagementOptions;
using Kritikos.AspNetCore.VersioningOptions;
using Kritikos.Extensions.Options.DependencyInjection;
using Kritikos.SemanticVersioning.DependencyInjection;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

using Scalar.AspNetCore;

public class Startup : IWebApplicationStartup
{
  // Single source for the API's OAuth2 scopes, shared by the OpenAPI scheme, Scalar, and the protected-resource metadata.
  private static readonly Dictionary<string, string> OAuthScopes = new(StringComparer.Ordinal)
  {
    ["openid"] = "Basic authentication",
    ["profile"] = "Access your profile",
    ["email"] = "Access your email",
  };

  // Maps ASP.NET Core authentication schemes to the OpenAPI security-scheme keys they are documented under.
  private static readonly Dictionary<string, string> AuthenticationSchemeMap = new(StringComparer.Ordinal)
  {
    [ApiKeyDefaults.AuthenticationScheme] = ApiKeySecuritySchemeDocumentTransformer.SchemeId,
  };

  /// <inheritdoc />
  public void ConfigureServices(WebApplicationBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);
    builder.Services.AddSemanticVersionDescriptor(typeof(Startup));

    builder.Services.AddEndpoints(typeof(Startup));
    builder.Services.AddValidation();
    builder.Services.AddMvcCore();

    builder.Services.AddCorrelationHeader();

    builder.Services.AddProblemDetails();

    builder.Services.AddFeatureManagement();
    builder.Services.AddHttpClient();

    builder.Services.AddOptionsDefinition<MyOpenApiInfoOptions>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => options.Authority = builder.Configuration["Authentication:OpenId:Authority"])
      .AddApiKey();
    builder.Services.AddAuthorization();
    builder.Services.AddSingleton<IApiKeyValidator, PetStoreApiKeyValidator>();

    builder.Services.AddApiVersioningDefaults(static set => set
      .HasApiVersion(new ApiVersion(1))
      .HasApiVersion(new ApiVersion(0))
      .HasApiVersion(new ApiVersion(3))
      .HasDeprecatedApiVersion(new ApiVersion(2))
      .ReportApiVersions());

    var authority = builder.Configuration["Authentication:OpenId:Authority"]
                    ?? throw new InvalidOperationException("Authentication:OpenId:Authority is not configured.");

    builder.Services
      .AddApiVersioning()
      .AddMvc()
      .AddOpenApi(options =>
      {
        options.Document.AddOperationTransformer(
          new AuthorizationCheckOperationTransformer(
            OAuth2SecuritySchemeDocumentTransformer.SchemeId, AuthenticationSchemeMap));
        options.Document.AddOAuth2SecurityScheme(
          new Uri($"{authority}/protocol/openid-connect/auth"),
          new Uri($"{authority}/protocol/openid-connect/token"),
          OAuthScopes);

        options.Document.AddApiKeySecurityScheme(ApiKeyDefaults.HeaderName);

        options.Document.AddQueryOperations(
          OAuth2SecuritySchemeDocumentTransformer.SchemeId, AuthenticationSchemeMap);

        options.Document.AddDocumentTransformer<InfoDocumentTransformer>();
        options.Document.AddDocumentTransformer<FeatureFilterDocumentTransformer>();
      });

    builder.Services.AddSecurityTxt(static options =>
      options.Expires = new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));

    // security.txt shares the OpenAPI info contact (OpenApi:Contact* config) as its single source.
    builder.Services
      .AddOptions<SecurityTxtOptions>()
      .Configure<IOptions<MyOpenApiInfoOptions>>(static (security, info) =>
        security.Contact.Add(new Uri($"mailto:{info.Value.ContactEmail}")));

    // api-catalog is populated from the registered API versions; oauth-protected-resource from the bearer authority.
    builder.Services.AddOidcProtectedResource(static options =>
    {
      foreach (var scope in OAuthScopes.Keys)
      {
        options.ScopesSupported.Add(scope);
      }

      options.BearerMethodsSupported.Add("header");
    });

    // ResourceName shares the OpenAPI info title as its single source.
    builder.Services
      .AddOptions<OAuthProtectedResourceOptions>()
      .Configure<IOptions<MyOpenApiInfoOptions>>(static (resource, info) =>
        resource.ResourceName = info.Value.Title);
  }

  /// <inheritdoc />
  public void Configure(WebApplication app)
  {
    ArgumentNullException.ThrowIfNull(app);

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapOpenApi().WithDocumentPerVersion();
    app.MapScalarApiReference(o =>
    {
      var authority = app.Configuration["Authentication:OpenId:Authority"];
      var clientId = app.Configuration["Authentication:OpenId:ClientId"] ?? string.Empty;

      o.AddDocuments(app.DescribeApiVersions().Select(static description => description.GroupName));
      o.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

      // The document declares the `oauth2` authorization-code scheme; Scalar adds the client id, PKCE, and
      // selected scopes so the interactive login authorizes the secured operations that require it.
      o.AddPreferredSecuritySchemes("oauth2");
      o.AddOAuth2Flows("oauth2", flows =>
      {
        flows.AuthorizationCode = new AuthorizationCodeFlow
        {
          ClientId = clientId,
          AuthorizationUrl = $"{authority}/protocol/openid-connect/auth",
          TokenUrl = $"{authority}/protocol/openid-connect/token",
          Pkce = Pkce.Sha256,
          SelectedScopes = [.. OAuthScopes.Keys],
        };
      });
    });

    app.UseCorrelationHeader();

    app.MapSecurityTxt();
    app.MapOAuthProtectedResource();
    app.MapApiCatalog();

    app.MapControllers();
    app.MapEndpoints();
  }
}
