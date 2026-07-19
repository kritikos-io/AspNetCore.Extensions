namespace Kritikos.SemanticVersioning;

/// <summary>
/// Describes any pre-release metadata found in a semantic version.
/// </summary>
/// <remarks>
/// This follows the specification, but additionally accounts for a commit counter as used by GitVersion.
/// </remarks>
public record PreReleaseMetadataDescriptor : IComparable<PreReleaseMetadataDescriptor>
{
  /// <summary>
  /// Initializes a new instance of the <see cref="PreReleaseMetadataDescriptor"/> class.
  /// </summary>
  internal PreReleaseMetadataDescriptor()
  {
  }

  /// <summary>
  /// Gets the pre-release tag identifier (e.g. "alpha", "beta", "rc").
  /// </summary>
  public string Tag { get; internal init; } = string.Empty;

  /// <summary>
  /// Gets the optional commit counter appended to the pre-release tag.
  /// </summary>
  public int? CommitCounter { get; internal init; }

  /// <summary>
  /// Gets or sets the original pre-release metadata string as it appeared in the version.
  /// </summary>
  internal string OriginalContent { get; set; } = string.Empty;

  /// <summary>Determines whether the left operand is greater than or equal to the right operand.</summary>
  /// <param name="left">The left operand.</param>
  /// <param name="right">The right operand.</param>
  /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>. Returns <see langword="false"/> when <paramref name="left"/> is <see langword="null"/>.</returns>
  public static bool operator >=(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left?.CompareTo(right) >= 0;

  /// <summary>Determines whether the left operand is less than or equal to the right operand.</summary>
  /// <param name="left">The left operand.</param>
  /// <param name="right">The right operand.</param>
  /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>. Returns <see langword="false"/> when <paramref name="left"/> is <see langword="null"/>.</returns>
  public static bool operator <=(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left?.CompareTo(right) <= 0;

  /// <summary>Determines whether the left operand is less than the right operand.</summary>
  /// <param name="left">The left operand.</param>
  /// <param name="right">The right operand.</param>
  /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>. Returns <see langword="false"/> when <paramref name="left"/> is <see langword="null"/>.</returns>
  public static bool operator <(PreReleaseMetadataDescriptor? left, PreReleaseMetadataDescriptor? right)
    => left?.CompareTo(right) < 0;

  /// <summary>Determines whether the left operand is greater than the right operand.</summary>
  /// <param name="left">The left operand.</param>
  /// <param name="right">The right operand.</param>
  /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>. Returns <see langword="false"/> when <paramref name="left"/> is <see langword="null"/>.</returns>
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
