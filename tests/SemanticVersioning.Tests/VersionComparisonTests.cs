namespace Kritikos.SemanticVersioning.Tests;

using Xunit;

public class VersionComparisonTests
{
  [Fact]
  public void Ordinal_versions_ordering_is_correct()
  {
    var version1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0");
    var version2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.1");
    var version3 = SemanticVersionDescriptor.FromInformationalVersion("1.1.0");
    var version4 = SemanticVersionDescriptor.FromInformationalVersion("2.0.0");

    Assert.True(version1 < version2);
    Assert.True(version2 < version3);
    Assert.True(version3 < version4);
  }

  [Fact]
  public void Ordinal_versions_equality_comparison_is_correct()
  {
    var version1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0");
    var version2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0");
    var version3 = SemanticVersionDescriptor.FromInformationalVersion("1.0.1");

    Assert.True(version1 == version2);
    Assert.True(version1 >= version2);

    Assert.True(version3 >= version2);
  }

  [Fact]
  public void Prerelease_version_comparison_is_correct()
  {
    var stableVersion = SemanticVersionDescriptor.FromInformationalVersion("1.0.0");
    var prereleaseVersion1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.1");
    var prereleaseVersion2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.2");
    var prereleaseVersion3 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-beta.1");

    Assert.True(prereleaseVersion1 < stableVersion);
    Assert.True(prereleaseVersion1 <= stableVersion);
    Assert.True(prereleaseVersion2 > prereleaseVersion1);
    Assert.True(prereleaseVersion3 > prereleaseVersion2);
  }
}
