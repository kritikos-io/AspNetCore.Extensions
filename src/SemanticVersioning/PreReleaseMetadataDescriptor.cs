namespace Kritikos.SemanticVersioning;

/// <summary>
/// Describes any pre-release metadata found in a semantic version.
/// </summary>
/// <remarks>
/// This follows the specification, but additionally accounts for a commit counter as used by GitVersion.
/// </remarks>
public record PreReleaseMetadataDescriptor : IComparable<PreReleaseMetadataDescriptor>
{
  internal PreReleaseMetadataDescriptor()
  {
  }

  internal string OriginalContent { get; set; } = string.Empty;

  public string Tag { get; internal init; } = string.Empty;

  public int? CommitCounter { get; internal init; }

  public static bool operator >=(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left?.CompareTo(right) >= 0;

  public static bool operator <=(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left?.CompareTo(right) <= 0;

  public static bool operator <(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left?.CompareTo(right) < 0;

  public static bool operator >(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left?.CompareTo(right) > 0;

  /// <inheritdoc />
  public int CompareTo(PreReleaseMetadataDescriptor? other)
    => ReferenceEquals(this, other)
        ? 0
        : other is null
            ? -1
            : string.CompareOrdinal(OriginalContent, other.OriginalContent);
}
