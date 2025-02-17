namespace Kritikos.AspNetCore.OpenApiOidcExtensions.Options;

public abstract class OpenApiOpenIdOptions
{
  public virtual string Authority { get; set; } = string.Empty;
}
