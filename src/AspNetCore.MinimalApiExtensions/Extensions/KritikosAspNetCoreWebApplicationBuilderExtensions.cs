namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Kritikos.AspNetCore.MinimalApiExtensions.StartupBuilder;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for <see cref="WebApplicationBuilder"/> and <see cref="HostApplicationBuilder"/> to support startup class patterns.
/// </summary>
public static class KritikosAspNetCoreWebApplicationBuilderExtensions
{
  /// <summary>
  /// Creates a <see cref="WebApplication"/> by configuring services and middleware using a <see cref="IWebApplicationStartup"/> class.
  /// </summary>
  /// <param name="builder">The <see cref="WebApplicationBuilder"/> to use in building the <see cref="WebApplication"/>.</param>
  /// <typeparam name="T">The type containing the startup methods for the application.</typeparam>
  /// <returns>The <see cref="WebApplication"/>.</returns>
  public static WebApplication UseStartup<T>(this WebApplicationBuilder builder)
      where T : class, IWebApplicationStartup, new()
  {
    ArgumentNullException.ThrowIfNull(builder);

    var startup = new T();
    startup.ConfigureServices(builder);

    var app = builder.Build();
    startup.Configure(app);

    return app;
  }

  /// <summary>
  /// Creates an <see cref="IHost"/> by configuring services and middleware using a <see cref="IApplicationStartup"/> class.
  /// </summary>
  /// <param name="builder">The <see cref="HostApplicationBuilder"/> to use in building the <see cref="WebApplication"/>.</param>
  /// <typeparam name="T">The type containing the startup methods for the application.</typeparam>
  /// <returns>The <see cref="IHost"/>.</returns>
  public static IHost UseStartup<T>(this HostApplicationBuilder builder)
      where T : class, IApplicationStartup, new()
  {
    ArgumentNullException.ThrowIfNull(builder);

    var startup = new T();

    startup.ConfigureServices(builder);
    var app = builder.Build();

    startup.Configure(app);

    return app;
  }
}
