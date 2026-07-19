namespace Kritikos.SemanticVersioning;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

/// <summary>
/// Provides constants and compiled regular expressions for semantic version string parsing.
/// </summary>
public static partial class SemanticVersioningConstants
{
  private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled;

  [StringSyntax(StringSyntaxAttribute.Regex)]
  private const string SemanticVersionPattern =
      @"^(?<Major>0|[1-9]\d*)\.(?<Minor>0|[1-9]\d*)\.(?<Patch>0|[1-9]\d*)(?<PreRelease>-(?<Tag>[^+]+?)\.?(?<Counter>0|[1-9]\d*)?)?(?<BuildMetadata>(\+Branch\.(?<Branch>.+?)\.Sha\.(?<Sha>.+?))|(\+.+))?$";

  [GeneratedRegex(SemanticVersionPattern, Options)]
  internal static partial Regex SemanticVersionMatcher();
}
