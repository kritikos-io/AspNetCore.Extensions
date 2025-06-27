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

  internal string OriginalContent { get; set; } = string.Empty;

  public string Branch { get; init; } = string.Empty;
  public string Sha1 { get; init; } = string.Empty;
}
