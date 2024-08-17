// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.MinimalApiExtensions.StartupBuilder;

public static partial class KritikosStartupBuilderExtensions
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

    var startup = Activator.CreateInstance<T>();
    startup.ConfigureServices(builder);

    var app = builder.Build();
    startup.Configure(app);

    return app;
  }

  /// <summary>
  /// Creates an <see cref="IHost"/> by configuring services and middleware using a <see cref="IWebApplicationStartup"/> class.
  /// </summary>
  /// <param name="builder">The <see cref="HostApplicationBuilder"/> to use in building the <see cref="WebApplication"/>.</param>
  /// <typeparam name="T">The type containing the startup methods for the application.</typeparam>
  /// <returns>The <see cref="IHost"/>.</returns>
  public static IHost UseStartup<T>(this HostApplicationBuilder builder)
      where T : class, IApplicationStartup, new()
  {
    ArgumentNullException.ThrowIfNull(builder);

    var startup = Activator.CreateInstance<T>();

    startup.ConfigureServices(builder);
    var app = builder.Build();

    startup.Configure(app);

    return app;
  }
}
