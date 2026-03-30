namespace Kritikos.SemanticVersioning.Tests;

public class VersionComparisonTests
{
  [Test]
  public async Task Ordinal_versions_are_compared_correctly()
  {
    var version1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0");
    var version2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.1");

    await Assert.That(version1 < version2).IsTrue();
    await Assert.That(version2 > version1).IsTrue();
    await Assert.That(version1 <= version2).IsTrue();
    await Assert.That(version2 >= version1).IsTrue();
  }

  [Test]
  public async Task Stable_version_is_greater_than_prerelease()
  {
    var stableVersion = SemanticVersionDescriptor.FromInformationalVersion("1.0.0");
    var prereleaseVersion = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.1");

    await Assert.That(stableVersion > prereleaseVersion).IsTrue();
  }

  [Test]
  public async Task Same_version_prereleases_are_ordered_according_to_prerelease_data()
  {
    var prereleaseVersion1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.1");
    var prereleaseVersion2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.15");

    await Assert.That(prereleaseVersion1 < prereleaseVersion2).IsTrue();
    await Assert.That(prereleaseVersion1.PreReleaseMetadata < prereleaseVersion2.PreReleaseMetadata).IsTrue();

    await Assert.That(prereleaseVersion1 with { PreReleaseMetadata = null }).IsEqualTo(prereleaseVersion2 with { PreReleaseMetadata = null });
  }

  [Test]
  public async Task Different_version_prereleases_are_ordered_according_to_ordinal_version_data()
  {
    var prereleaseVersion1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.1-alpha.1");
    var prereleaseVersion2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0-alpha.15");

    await Assert.That(prereleaseVersion1 > prereleaseVersion2).IsTrue();
    await Assert.That(prereleaseVersion1.PreReleaseMetadata < prereleaseVersion2.PreReleaseMetadata).IsTrue();

    await Assert.That(prereleaseVersion1 with { PreReleaseMetadata = null } > prereleaseVersion2 with { PreReleaseMetadata = null }).IsTrue();
  }

  [Test]
  public async Task BuildMetadata_is_ignored_in_version_comparison()
  {
    var version1 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0+Branch.main.Sha.1234567890abcdef");
    var version2 = SemanticVersionDescriptor.FromInformationalVersion("1.0.0+Branch.main.Sha.abcdef1234567890");

    await Assert.That(version1 == version2).IsTrue();
    await Assert.That(version1.BuildMetadata).IsNotEqualTo(version2.BuildMetadata);
  }
}
