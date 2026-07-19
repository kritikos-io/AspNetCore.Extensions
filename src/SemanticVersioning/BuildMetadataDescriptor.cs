namespace Kritikos.SemanticVersioning;

/// <summary>
/// Describes the build metadata segment extracted from a semantic version string.
/// </summary>
/// <remarks>
/// This is tailored for use in versions calculated by GitVersion, and should follow the format 'Branch.{branchName}.Sha.{sha1Hash}'.
/// </remarks>
public record BuildMetadataDescriptor
{
  /// <summary>
  /// Initializes a new instance of the <see cref="BuildMetadataDescriptor"/> class.
  /// </summary>
  internal BuildMetadataDescriptor()
  {
  }

  /// <summary>
  /// Gets the branch name from the build metadata.
  /// </summary>
  public string Branch { get; init; } = string.Empty;

  /// <summary>
  /// Gets the SHA-1 commit hash from the build metadata.
  /// </summary>
  public string Sha1 { get; init; } = string.Empty;

  /// <summary>
  /// Gets or sets the original build metadata string as it appeared in the version.
  /// </summary>
  internal string OriginalContent { get; set; } = string.Empty;
}
