// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using Kritikos.AspNetCore.MinimalApiExtensions.Startup;

public static partial class KritikosIEndpointExtensions
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
}
