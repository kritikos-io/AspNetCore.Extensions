namespace Kritikos.AspNetCore.MinimalApiExtensions.Startup;

/// <summary>
/// Provides an interface for initializing services and middleware used by an application.
/// </summary>
[CLSCompliant(false)]
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
