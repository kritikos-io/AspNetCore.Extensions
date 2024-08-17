namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

public static class KritikosOptionsDefinitionExtensions
{
  public static IServiceCollection AddOptionsDefinition<TOptions>(this IServiceCollection services)
      where TOptions : class, IOptionsDefinition, new()
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOptions<TOptions>()
        .BindConfiguration(TOptions.Location)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    return services;
  }
}
