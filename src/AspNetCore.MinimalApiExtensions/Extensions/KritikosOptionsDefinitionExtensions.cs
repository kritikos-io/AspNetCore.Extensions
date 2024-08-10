namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

public static class KritikosOptionsDefinitionExtensions
{
  public static void AddOptionsDefinition<TOptions>(this WebApplicationBuilder builder)
      where TOptions : class, IOptionsDefinition, new()
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.Services.Configure<TOptions>(builder.Configuration.GetSection(TOptions.Location));
  }
}
