namespace Kritikos.SemanticVersioning;

public record BuildMetadataDescriptor
{
  internal string OriginalContent { get; set; } = string.Empty;

  public string Branch { get; init; } = string.Empty;
  public string Sha1 { get; init; } = string.Empty;

  public static BuildMetadataDescriptor? FromBuildPart(string buildPart)
  {
    var match = SemanticVersioningConstants.BuildMetadataMatcher().Match(buildPart);

    var result = match.Success
        ? new BuildMetadataDescriptor { Branch = match.Groups["Branch"].Value, Sha1 = match.Groups["Sha"].Value, OriginalContent = buildPart }
        : null;

    return result;
  }
}
