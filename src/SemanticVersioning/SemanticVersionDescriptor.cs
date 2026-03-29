namespace Kritikos.SemanticVersioning;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;

/// <summary>
/// Describes a version according to the <see href="https://semver.org">Semantic Versioning 2.0.0</see> specification.
///
/// </summary>
public record SemanticVersionDescriptor : IComparable<SemanticVersionDescriptor>
{
  private const char VersionSeperator = '.';

  private string saneVersion = string.Empty;

  private SemanticVersionDescriptor()
  {
  }

  /// <summary>
  /// Gets the Major version number.
  /// </summary>
  /// <remarks>
  /// Changes denote backward-incompatible API changes.
  /// </remarks>
  public int Major { get; internal init; }

  /// <summary>
  /// Gets the Minor version number.
  /// </summary>
  /// <remarks>
  /// Changes denote backward-compatible functionality.
  /// </remarks>
  public int Minor { get; internal init; }

  /// <summary>
  /// Gets the Patch version number.
  /// </summary>
  /// <remarks>
  /// Changes denote backward-compatible bug fixes.
  /// </remarks>
  public int Patch { get; internal init; }

  /// <summary>
  /// Gets the optional pre-release metadata.
  /// </summary>
  public PreReleaseMetadataDescriptor? PreReleaseMetadata { get; init; }

  /// <summary>
  /// Gets the optional build metadata.
  /// </summary>
  public BuildMetadataDescriptor? BuildMetadata { get; init; }

  /// <summary>Determines whether the left operand is greater than or equal to the right operand.</summary>
  /// <param name="left">The left operand.</param>
  /// <param name="right">The right operand.</param>
  /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
  public static bool operator >=(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
  {
    ArgumentNullException.ThrowIfNull(left);
    ArgumentNullException.ThrowIfNull(right);

    return left.CompareTo(right) >= 0;
  }

  /// <summary>Determines whether the left operand is less than or equal to the right operand.</summary>
  /// <param name="left">The left operand.</param>
  /// <param name="right">The right operand.</param>
  /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
  public static bool operator <=(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
  {
    ArgumentNullException.ThrowIfNull(left);
    ArgumentNullException.ThrowIfNull(right);

    return left.CompareTo(right) <= 0;
  }

  /// <summary>Determines whether the left operand is less than the right operand.</summary>
  /// <param name="left">The left operand.</param>
  /// <param name="right">The right operand.</param>
  /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
  public static bool operator <(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
  {
    ArgumentNullException.ThrowIfNull(left);
    ArgumentNullException.ThrowIfNull(right);

    return left.CompareTo(right) < 0;
  }

  /// <summary>Determines whether the left operand is greater than the right operand.</summary>
  /// <param name="left">The left operand.</param>
  /// <param name="right">The right operand.</param>
  /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
  public static bool operator >(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
  {
    ArgumentNullException.ThrowIfNull(left);
    ArgumentNullException.ThrowIfNull(right);

    return left.CompareTo(right) > 0;
  }

  /// <summary>
  /// Creates a <see cref="SemanticVersionDescriptor"/> from the assembly containing the specified type.
  /// </summary>
  /// <param name="type">The type whose assembly provides the version information.</param>
  /// <returns>A <see cref="SemanticVersionDescriptor"/> parsed from the assembly's informational version.</returns>
  [ExcludeFromCodeCoverage]
  public static SemanticVersionDescriptor FromType(Type? type) =>
    FromAssembly(type?.Assembly ?? throw new ArgumentNullException(nameof(type)));

  /// <summary>
  /// Creates a <see cref="SemanticVersionDescriptor"/> from the specified assembly.
  /// </summary>
  /// <param name="assembly">The assembly providing the version information.</param>
  /// <returns>A <see cref="SemanticVersionDescriptor"/> parsed from the assembly's informational version.</returns>
  [ExcludeFromCodeCoverage]
  public static SemanticVersionDescriptor FromAssembly(Assembly assembly)
  {
    var informationalVersion =
      assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
    return FromInformationalVersion(informationalVersion);
  }

  /// <summary>
  /// Parses an informational version string into a <see cref="SemanticVersionDescriptor"/>.
  /// </summary>
  /// <param name="informationalVersion">The informational version string to parse.</param>
  /// <returns>A <see cref="SemanticVersionDescriptor"/> representing the parsed version.</returns>
  /// <exception cref="ArgumentException">The version string is not in a valid semantic versioning format.</exception>
  public static SemanticVersionDescriptor FromInformationalVersion(string informationalVersion)
  {
    var match = SemanticVersioningConstants.SemanticVersionMatcher().Match(informationalVersion);

    if (!match.Success)
    {
      throw new ArgumentException(
        "The provided version string is not in a valid format.",
        nameof(informationalVersion));
    }

    var major = int.Parse(match.Groups["Major"].Value, CultureInfo.InvariantCulture);
    var minor = int.Parse(match.Groups["Minor"].Value, CultureInfo.InvariantCulture);
    var patch = int.Parse(match.Groups["Patch"].Value, CultureInfo.InvariantCulture);

    var prerelease = string.IsNullOrEmpty(match.Groups["PreRelease"].Value)
      ? null
      : new PreReleaseMetadataDescriptor()
      {
        Tag = match.Groups["Tag"].Value,
        CommitCounter = match.Groups["Counter"].Success
          ? int.Parse(match.Groups["Counter"].Value, CultureInfo.InvariantCulture)
          : null,
        OriginalContent = match.Groups["PreRelease"].Value,
      };

    var buildMetadata = string.IsNullOrEmpty(match.Groups["BuildMetadata"].Value)
      ? null
      : new BuildMetadataDescriptor
      {
        Branch = match.Groups["Branch"].Value,
        Sha1 = match.Groups["Sha"].Value,
        OriginalContent = match.Groups["BuildMetadata"].Value,
      };

    return new SemanticVersionDescriptor
    {
      Major = major,
      Minor = minor,
      Patch = patch,
      PreReleaseMetadata = prerelease,
      BuildMetadata = buildMetadata,
    };
  }

  /// <inheritdoc />
  public override string ToString()
  {
    if (!string.IsNullOrWhiteSpace(saneVersion))
    {
      return saneVersion;
    }

    var stringBuilder = new StringBuilder(6);
    stringBuilder.Append(Major);
    stringBuilder.Append(VersionSeperator);
    stringBuilder.Append(Minor);
    stringBuilder.Append(VersionSeperator);
    stringBuilder.Append(Patch);

    if (PreReleaseMetadata is not null)
    {
      stringBuilder.Append(PreReleaseMetadata.OriginalContent);
    }

    saneVersion = stringBuilder.ToString();
    return saneVersion;
  }

  /// <inheritdoc />
  public int CompareTo(SemanticVersionDescriptor? other)
  {
    int result;

    return ReferenceEquals(this, other)
      ? 0
      : other is null
        ? -1
        : (result = Major.CompareTo(other.Major)) != 0
          || (result = Minor.CompareTo(other.Minor)) != 0
          || (result = Patch.CompareTo(other.Patch)) != 0
          ? result
          : PreReleaseMetadata?.CompareTo(other.PreReleaseMetadata) ?? 1;
  }

  /// <inheritdoc />
  public virtual bool Equals(SemanticVersionDescriptor? other)
  {
    return ReferenceEquals(this, other)
           || (other is not null
               && Major == other.Major
               && Minor == other.Minor
               && Patch == other.Patch
               && PreReleaseMetadata == other.PreReleaseMetadata);
  }

  /// <inheritdoc />
  public override int GetHashCode()
    => HashCode.Combine(Major, Minor, Patch, PreReleaseMetadata);
}
