namespace Kritikos.SemanticVersioning;

using System.Globalization;
using System.Reflection;
using System.Text;

public record SemanticVersionDescriptor
{
  private const char VersionSeperator = '.';
  private const char PrereleaseSeperator = '-';

  internal string saneVersion = string.Empty;

  public int Major { get; init; }

  public int Minor { get; init; }

  public int Patch { get; init; }

  public PreReleaseMetadataDescriptor? PreReleaseMetadata { get; init; }

  public BuildMetadataDescriptor? BuildMetadata { get; init; }

  public static SemanticVersionDescriptor FromType(Type type) => FromAssembly(type.Assembly);

  public static SemanticVersionDescriptor FromAssembly(Assembly assembly)
  {
    var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
    return FromInformationalVersion(informationalVersion);
  }

  public static SemanticVersionDescriptor FromInformationalVersion(string informationalVersion)
  {
    var match = SemanticVersioningConstants.VersionMatcher().Match(informationalVersion);
    if (!match.Success)
    {
      throw new ArgumentException("The provided version string is not in a valid format.", nameof(informationalVersion));
    }

    var major = int.Parse(match.Groups["Major"].Value, CultureInfo.InvariantCulture);
    var minor = int.Parse(match.Groups["Minor"].Value, CultureInfo.InvariantCulture);
    var patch = int.Parse(match.Groups["Patch"].Value, CultureInfo.InvariantCulture);

    var prerelease = string.IsNullOrEmpty(match.Groups["Prerelease"].Value)
        ? null
        : PreReleaseMetadataDescriptor.FromPreReleasePart(match.Groups["Prerelease"].Value);

    var buildMetadata = string.IsNullOrEmpty(match.Groups["BuildMetadata"].Value)
        ? null
        : BuildMetadataDescriptor.FromBuildPart(match.Groups["BuildMetadata"].Value);

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
      stringBuilder.Append(PrereleaseSeperator);
      stringBuilder.Append(PreReleaseMetadata.OriginalContent);
    }

    saneVersion = stringBuilder.ToString();
    return saneVersion;
  }

  public static bool operator >=(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
    => left.Equals(right) || left > right;

  public static bool operator <=(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
    => left.Equals(right) || left < right;

  public static bool operator <(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
    => !(left > right);

  public static bool operator >(SemanticVersionDescriptor left, SemanticVersionDescriptor right)
  {
    if (left.Equals(right))
    {
      return left.PreReleaseMetadata > right.PreReleaseMetadata;
    }

    var isGreater =
        left.Major > right.Major
        || (left.Major == right.Major && left.Minor > right.Minor)
        || (left.Major == right.Major && left.Minor == right.Minor && left.Patch > right.Patch)
        || (left.Major == right.Major && left.Minor == right.Minor && left.Patch == right.Patch && left.PreReleaseMetadata > right.PreReleaseMetadata);

    return isGreater;
  }
}
