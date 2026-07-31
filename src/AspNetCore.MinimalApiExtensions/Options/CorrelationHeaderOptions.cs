namespace Kritikos.AspNetCore.MinimalApiExtensions.Options;

using System.ComponentModel.DataAnnotations;

using Kritikos.Extensions.Options.Contracts;

/// <summary>
/// Configuration options for the correlation header middleware.
/// </summary>
public sealed class CorrelationHeaderOptions : IOptionsDefinition, IValidatableObject
{
  /// <summary>
  /// The default correlation header name.
  /// </summary>
  public const string DefaultHeader = "X-Correlation-Id";

  /// <inheritdoc />
  public static string Location { get; } = "AspNetCore:Middleware:Correlation";

  /// <summary>
  /// Gets or sets the name of the correlation header. Defaults to <see cref="DefaultHeader"/>.
  /// </summary>
  public string Header { get; set; } = DefaultHeader;

  /// <summary>
  /// Gets or sets a value indicating whether the correlation identifier should be included in the HTTP response.
  /// </summary>
  public bool IncludeInResponse { get; set; } = true;

  /// <inheritdoc />
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(Header))
    {
      yield return new ValidationResult(
        "Header must not be empty or whitespace.",
        [nameof(Header)]);
    }
  }
}
