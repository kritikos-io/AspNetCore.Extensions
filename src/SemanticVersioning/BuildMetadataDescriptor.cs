namespace Kritikos.SemanticVersioning;

/// <summary>
/// Describes metadata picked up during build.
/// </summary>
/// <remarks>
/// This is tailored for use in versions calculated by GitVersion, and should follow the format 'Branch.{branchName}.Sha.{sha1Hash}'.
/// </remarks>
public record BuildMetadataDescriptor
{
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

  internal string OriginalContent { get; set; } = string.Empty;
}
