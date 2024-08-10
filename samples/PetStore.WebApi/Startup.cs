namespace Kritikos.PetStore.WebApi;

using Asp.Versioning;

using Kritikos.AspNetCore.MinimalApiExtensions.Startup;

using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;

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

    builder.Services.AddSwaggerDefaults();
    builder.Services.AddSwaggerGen();

    builder.Services.AddProblemDetails();

    builder.Services.AddFeatureManagement();
    builder.Services.AddSwaggerFeatureManagementDefaults();

    builder.Services.AddRedocDefaults();
    builder.Services.AddApiVersioningDefaults();
    builder.Services.AddSwaggerVersioning(new OpenApiInfo() { Title = "PetStore API", });
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

    app.UseSwagger();
    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCorrelationHeader();
    app.MapEndpoints();

    app.UseVersionedSwaggerUI();
    app.UseVersionedReDoc();
  }
}
