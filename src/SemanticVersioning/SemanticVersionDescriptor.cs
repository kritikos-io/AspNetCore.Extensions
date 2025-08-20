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
  /// Changes denote backward-incompatible API changes.
  /// </summary>
  public int Major { get; internal init; }

  /// <summary>
  /// Changes denote backward-compatible functionality.
  /// </summary>
  public int Minor { get; internal init; }

  /// <summary>
  /// Changes denote backward-compatible bug fixes.
  /// </summary>
  public int Patch { get; internal init; }

  public PreReleaseMetadataDescriptor? PreReleaseMetadata { get; init; }

  public BuildMetadataDescriptor? BuildMetadata { get; init; }

  [ExcludeFromCodeCoverage]
  public static SemanticVersionDescriptor FromType(Type type) => FromAssembly(type.Assembly);

  [ExcludeFromCodeCoverage]
  public static SemanticVersionDescriptor FromAssembly(Assembly assembly)
  {
    var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
    return FromInformationalVersion(informationalVersion);
  }

  public static SemanticVersionDescriptor FromInformationalVersion(string informationalVersion)
  {
    var match = SemanticVersioningConstants.SemanticVersionMatcher().Match(informationalVersion);

    if (!match.Success)
    {
      throw new ArgumentException("The provided version string is not in a valid format.", nameof(informationalVersion));
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
        : new BuildMetadataDescriptor { Branch = match.Groups["Branch"].Value, Sha1 = match.Groups["Sha"].Value, OriginalContent = match.Groups["BuildMetadata"].Value };

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

  public static bool operator >=(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
    => left.CompareTo(right) >= 0;

  public static bool operator <=(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
    => left.CompareTo(right) <= 0;

  public static bool operator <(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
    => left.CompareTo(right) < 0;

  public static bool operator >(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
    => left.CompareTo(right) > 0;

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

  public override int GetHashCode()
    => HashCode.Combine(Major, Minor, Patch, PreReleaseMetadata);
}
