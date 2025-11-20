namespace Kritikos.AspNetCore.VersioningOptions;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;

using Kritikos.AspNetCore.VersioningOptions.Contracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class KritikosApiVersioningDependencyInjectionExtensions
{
  /// <param name="services"><see cref="IServiceCollection"/> to configure.</param>
  extension(IServiceCollection services)
  {
    /// <summary>
    /// Adds opinionated default options for api versioning.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> are null.</exception>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    public IServiceCollection AddApiVersioningDefaults()
    {
      ArgumentNullException.ThrowIfNull(services);

      services
        .AddApiVersioning()
        .AddApiExplorer()
        .EnableApiVersionBinding();

      services.ConfigureOptions<ApiVersioningDefaultOptions>();

      return services;
    }

    public IServiceCollection AddApiVersionModelProvider<TVersionModelProvider>()
      where TVersionModelProvider : IApiVersionModelProvider
    {
      ArgumentNullException.ThrowIfNull(services);
      services.AddSingleton(TVersionModelProvider.VersionModel);
      return services;
    }

    public IServiceCollection AddApiVersionSetProvider<TVersionSetProvider>()
      where TVersionSetProvider : IApiVersionSetProvider
    {
      ArgumentNullException.ThrowIfNull(services);
      services.AddSingleton<ApiVersionSet>(static _ => TVersionSetProvider.VersionSet);
      return services;
    }
  }

  extension(WebApplication app)
  {
    public WebApplication AddApiVersionSet<TVersionSetProvider>(
      Action<ApiVersionSetBuilder>? setupAction = null)
      where TVersionSetProvider : IApiVersionSetProvider
    {
      ArgumentNullException.ThrowIfNull(app);
      var versionModel = app.Services.GetRequiredService<ApiVersionModel>();

      var set = app.NewApiVersionSet()
        .HasApiVersions(versionModel.SupportedApiVersions)
        .HasDeprecatedApiVersions(versionModel.DeprecatedApiVersions);

      setupAction?.Invoke(set);
      TVersionSetProvider.VersionSet = set.Build();

      return app;
    }
  }
}
