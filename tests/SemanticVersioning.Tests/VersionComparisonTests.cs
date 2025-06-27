namespace Kritikos.SemanticVersioning.Tests;

public class VersionComparisonTests
{
  [Fact]
  public void Ordinal_versions_are_compared_correctly()
  {
    var version1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0");
    var version2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.1");

    Assert.True(version1 < version2);
    Assert.True(version2 > version1);
    Assert.True(version1 <= version2);
    Assert.True(version2 >= version1);
  }

  [Fact]
  public void Stable_version_is_greater_than_prerelease()
  {
    var stableVersion = SemanticVersionDescriptor.FromInformationalVersion("1.0.0");
    var prereleaseVersion = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.1");

    Assert.True(stableVersion > prereleaseVersion);
  }

  [Fact]
  public void Same_version_prereleases_are_ordered_according_to_prerelease_data()
  {
    var prereleaseVersion1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.1");
    var prereleaseVersion2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.15");

    Assert.True(prereleaseVersion1 < prereleaseVersion2);
    Assert.True(prereleaseVersion1.PreReleaseMetadata < prereleaseVersion2.PreReleaseMetadata);

    Assert.Equal(prereleaseVersion1 with { PreReleaseMetadata = null }, prereleaseVersion2 with { PreReleaseMetadata = null });
  }

  [Fact]
  public void Different_version_prereleases_are_ordered_according_to_ordinal_version_data()
  {
    var prereleaseVersion1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.1-alpha.1");
    var prereleaseVersion2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.15");

    Assert.True(prereleaseVersion1 > prereleaseVersion2);
    Assert.True(prereleaseVersion1.PreReleaseMetadata < prereleaseVersion2.PreReleaseMetadata);

    Assert.True(prereleaseVersion1 with { PreReleaseMetadata = null } > prereleaseVersion2 with { PreReleaseMetadata = null });
  }

  [Fact]
  public void BuildMetadata_is_ignored_in_version_comparison()
  {
    var version1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0+Branch.main.Sha.1234567890abcdef");
    var version2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0+Branch.main.Sha.abcdef1234567890");

    Assert.True(version1 == version2);
    Assert.NotEqual(version1.BuildMetadata, version2.BuildMetadata);
  }
}
