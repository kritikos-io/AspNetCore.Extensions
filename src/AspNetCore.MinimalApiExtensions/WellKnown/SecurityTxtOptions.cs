namespace Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using System.ComponentModel.DataAnnotations;

using Kritikos.Extensions.Options.Contracts;

/// <summary>
/// Configuration for the RFC 9116 <c>/.well-known/security.txt</c> endpoint.
/// </summary>
public sealed class SecurityTxtOptions : IOptionsDefinition, IValidatableObject
{
  /// <inheritdoc />
  public static string Location { get; } = "AspNetCore:WellKnown:SecurityTxt";

  /// <summary>
  /// Gets the contact methods researchers should use, in order of preference. At least one is required.
  /// </summary>
  public IList<Uri> Contact { get; } = [];

  /// <summary>
  /// Gets or sets the date after which the information in the document is considered stale. Required by RFC 9116.
  /// </summary>
  public DateTimeOffset? Expires { get; set; }

  /// <summary>Gets the URIs of encryption keys researchers should use for secure communication.</summary>
  public IList<Uri> Encryption { get; } = [];

  /// <summary>Gets the URIs of pages that acknowledge security researchers.</summary>
  public IList<Uri> Acknowledgments { get; } = [];

  /// <summary>Gets the URIs of the vulnerability disclosure policy pages.</summary>
  public IList<Uri> Policy { get; } = [];

  /// <summary>Gets the URIs of security-related job postings.</summary>
  public IList<Uri> Hiring { get; } = [];

  /// <summary>Gets the canonical URIs where this document is located.</summary>
  public IList<Uri> Canonical { get; } = [];

  /// <summary>Gets the preferred natural languages (RFC 5646 tags) for security reports.</summary>
  public IList<string> PreferredLanguages { get; } = [];

  /// <inheritdoc />
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Contact.Count == 0)
    {
      yield return new("At least one Contact is required.", [nameof(Contact)]);
    }

    if (Expires is null)
    {
      yield return new("Expires is required.", [nameof(Expires)]);
    }

    foreach (var result in InsecureUris(Contact, nameof(Contact)))
    {
      yield return result;
    }

    foreach (var result in InsecureUris(Encryption, nameof(Encryption)))
    {
      yield return result;
    }

    foreach (var result in InsecureUris(Acknowledgments, nameof(Acknowledgments)))
    {
      yield return result;
    }

    foreach (var result in InsecureUris(Policy, nameof(Policy)))
    {
      yield return result;
    }

    foreach (var result in InsecureUris(Hiring, nameof(Hiring)))
    {
      yield return result;
    }

    foreach (var result in InsecureUris(Canonical, nameof(Canonical)))
    {
      yield return result;
    }
  }

  private static IEnumerable<ValidationResult> InsecureUris(IList<Uri> uris, string member)
  {
    foreach (var uri in uris)
    {
      if (uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeHttp)
      {
        yield return new($"{member} web URIs must use https, not http.", [member]);
      }
    }
  }
}
