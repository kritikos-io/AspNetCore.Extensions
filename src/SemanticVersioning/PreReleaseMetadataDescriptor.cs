namespace Kritikos.SemanticVersioning;

using System.Globalization;

public record PreReleaseMetadataDescriptor
{
  internal string OriginalContent { get; set; } = string.Empty;

  public string Tag { get; init; } = string.Empty;
  public int? CommitCounter { get; init; }

  public static PreReleaseMetadataDescriptor? FromPreReleasePart(string preReleasePart)
  {
    var match = SemanticVersioningConstants.PreReleaseMatcher().Match(preReleasePart);

    var result = match.Success
        ? new PreReleaseMetadataDescriptor
        {
          Tag = match.Groups["Tag"].Value,
          CommitCounter = match.Groups["Counter"].Success
              ? int.Parse(match.Groups["Counter"].Value, CultureInfo.InvariantCulture)
              : null,
          OriginalContent = preReleasePart,
        }
        : null;

    return result;
  }

  public static bool operator >=(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left == right || left > right;

  public static bool operator <=(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left == right || left < right;

  public static bool operator <(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => !(left > right);

  public static bool operator >(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
  {
    return left switch
    {
      null when right is not null => true,
      not null when right is null => false,
      null when right is null => false,
      _ => string.Compare(left!.OriginalContent, right.OriginalContent, StringComparison.Ordinal) > 0,
    };
  }
}
