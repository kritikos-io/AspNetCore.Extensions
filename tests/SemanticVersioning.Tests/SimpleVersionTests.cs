namespace Kritikos.SemanticVersioning.Tests;

public class SimpleVersionTests
{
  [Test]
  public async Task Build_metadata_version_is_parsed()
  {
    const string version = "1.0.3+Branch.main.Sha.186bfb5a230fb8821286f99e8b28bd4d32433c91";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    await Assert.That(semanticVersion.Major).IsEqualTo(1);
    await Assert.That(semanticVersion.Minor).IsEqualTo(0);
    await Assert.That(semanticVersion.Patch).IsEqualTo(3);
    await Assert.That(semanticVersion.PreReleaseMetadata).IsNull();
    await Assert.That(semanticVersion.BuildMetadata).IsNotNull();
    await Assert.That(semanticVersion.BuildMetadata!.Branch).IsEqualTo("main");
    await Assert.That(semanticVersion.BuildMetadata.Sha1).IsEqualTo("186bfb5a230fb8821286f99e8b28bd4d32433c91");
  }

  [Test]
  public async Task Ordinal_version_with_only_major_minor_patch_is_parsed()
  {
    const string version = "1.2.3";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    await Assert.That(semanticVersion.Major).IsEqualTo(1);
    await Assert.That(semanticVersion.Minor).IsEqualTo(2);
    await Assert.That(semanticVersion.Patch).IsEqualTo(3);
    await Assert.That(semanticVersion.PreReleaseMetadata).IsNull();
    await Assert.That(semanticVersion.BuildMetadata).IsNull();
  }

  [Test]
  public async Task Version_with_prerelease_only_is_parsed()
  {
    const string version = "1.2.1-rc.72";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    await Assert.That(semanticVersion.Major).IsEqualTo(1);
    await Assert.That(semanticVersion.Minor).IsEqualTo(2);
    await Assert.That(semanticVersion.Patch).IsEqualTo(1);
    await Assert.That(semanticVersion.PreReleaseMetadata?.Tag).IsEqualTo("rc");
    await Assert.That(semanticVersion.PreReleaseMetadata?.CommitCounter).IsEqualTo(72);
    await Assert.That(semanticVersion.BuildMetadata).IsNull();
  }

  [Test]
  public async Task Version_with_build_metadata_only_is_parsed()
  {
    const string version = "1.2.1+Branch.main.Sha.186bfb5a230fb8821286f99e8b28bd4d32433c91";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    await Assert.That(semanticVersion.Major).IsEqualTo(1);
    await Assert.That(semanticVersion.Minor).IsEqualTo(2);
    await Assert.That(semanticVersion.Patch).IsEqualTo(1);
    await Assert.That(semanticVersion.PreReleaseMetadata).IsNull();
    await Assert.That(semanticVersion.BuildMetadata).IsNotNull();
    await Assert.That(semanticVersion.BuildMetadata!.Branch).IsEqualTo("main");
    await Assert.That(semanticVersion.BuildMetadata.Sha1).IsEqualTo("186bfb5a230fb8821286f99e8b28bd4d32433c91");
  }

  [Test]
  public async Task Full_version_is_parsed()
  {
    const string version = "1.2.1-rc.72+Branch.main.Sha.186bfb5a230fb8821286f99e8b28bd4d32433c91";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    await Assert.That(semanticVersion.Major).IsEqualTo(1);
    await Assert.That(semanticVersion.Minor).IsEqualTo(2);
    await Assert.That(semanticVersion.Patch).IsEqualTo(1);
    await Assert.That(semanticVersion.PreReleaseMetadata?.Tag).IsEqualTo("rc");
    await Assert.That(semanticVersion.PreReleaseMetadata?.CommitCounter).IsEqualTo(72);
    await Assert.That(semanticVersion.BuildMetadata?.Branch).IsEqualTo("main");
    await Assert.That(semanticVersion.BuildMetadata?.Sha1).IsEqualTo("186bfb5a230fb8821286f99e8b28bd4d32433c91");
  }

  [Test]
  public async Task Version_with_invalid_format_throws_exception()
  {
    const string invalidVersion = "1.0";

    await Assert.ThrowsAsync<ArgumentException>(() =>
    {
      _ = SemanticVersionDescriptor.FromInformationalVersion(invalidVersion);
      return Task.CompletedTask;
    });
  }
}
