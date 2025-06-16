namespace Kritikos.SemanticVersioning;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

public static partial class SemanticVersioningConstants
{
  private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled;

  [StringSyntax(StringSyntaxAttribute.Regex)]
  private const string SemverPattern =
      @"^(?<Major>0|[1-9]\d*)\.(?<Minor>0|[1-9]\d*)\.(?<Patch>0|[1-9]\d*)(-(?<Prerelease>(0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(\.(0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(\+(?<BuildMetadata>[0-9a-zA-Z-]+(\.[0-9a-zA-Z-]+)*))?$";

  [StringSyntax(StringSyntaxAttribute.Regex)]
  private const string PrereleasePattern = @"^(?<Tag>.+?)(\.(?<Counter>0|[1-9]\d*))?$";

  [StringSyntax(StringSyntaxAttribute.Regex)]
  private const string BuildMetadataPattern = @"^Branch\.(?<Branch>.+?)\.Sha\.(?<Sha>.+?)$";

  [GeneratedRegex(PrereleasePattern, Options)]
  public static partial Regex PreReleaseMatcher();

  [GeneratedRegex(SemverPattern, Options)]
  public static partial Regex VersionMatcher();

  [GeneratedRegex(BuildMetadataPattern, Options)]
  public static partial Regex BuildMetadataMatcher();
}
