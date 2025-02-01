namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.Extensions.DependencyInjection;

public static class KritikosOptionsDefinitionExtensions
{
  public static IServiceCollection AddOptionsDefinition<TOptions>(this IServiceCollection services)
      where TOptions : class, IOptionsDefinition, new()
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddOptionsWithValidateOnStart<TOptions>()
        .BindConfiguration(TOptions.Location)
        .ValidateDataAnnotations();

    return services;
  }
}
