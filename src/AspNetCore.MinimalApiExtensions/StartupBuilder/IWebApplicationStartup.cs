namespace Kritikos.AspNetCore.MinimalApiExtensions.StartupBuilder;

using Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides a startup contract for configuring services and middleware on a <see cref="WebApplicationBuilder"/>.
/// </summary>
public interface IWebApplicationStartup
{
  /// <summary>
  /// Register services into the <see cref="WebApplicationBuilder.Services"/> container.
  /// </summary>
  /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure services for.</param>
  void ConfigureServices(WebApplicationBuilder builder);

  /// <summary>
  /// Configures middleware on a <see cref="WebApplication"/>.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  void Configure(WebApplication app);
}
