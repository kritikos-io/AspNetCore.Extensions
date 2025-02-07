namespace Kritikos.AspNetCore.MinimalApiExtensions.StartupBuilder;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Provides an interface for initializing services and middleware used by an application.
/// </summary>
[CLSCompliant(false)]
public interface IApplicationStartup
{
  /// <summary>
  /// Register services into the <see cref="WebApplicationBuilder.Services"/> container.
  /// </summary>
  /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure services for.</param>
  void ConfigureServices(IHostApplicationBuilder builder);

  /// <summary>
  /// Configures middleware on a <see cref="WebApplication"/>.
  /// </summary>
  /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
  void Configure(IHost app);
}
