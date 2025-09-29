namespace Kritikos.PetStore.WebApi;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;
using Kritikos.AspNetCore.OpenApiOidcExtensions.Options;
using Kritikos.Extensions.Options.Contracts;

public class MyOpenApiOpenIdOptions : OpenApiOpenIdOptions, IOptionsDefinition
{
  /// <inheritdoc />
  public static string Location { get; } = "Authentication:OpenId";

  public string ClientId { get; set; } = string.Empty;
}
