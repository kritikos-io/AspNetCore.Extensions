namespace Kritikos.AspNetCore.MinimalApiExtensions.Options;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

public class CorrelationHeaderOptions : IOptionsDefinition
{
  public const string DefaultHeader = "X-Correlation-Id";

  /// <inheritdoc />
  public static string Location { get; } = "AspNetCore:Middleware:Correlation";

  public string Header { get; set; } = DefaultHeader;

  public bool IncludeInResponse { get; set; } = true;
}
