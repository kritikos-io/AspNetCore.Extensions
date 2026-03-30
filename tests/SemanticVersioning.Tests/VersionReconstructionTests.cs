namespace Kritikos.SemanticVersioning.Tests;

public class VersionReconstructionTests
{
  private const int Major = 1;
  private const int Minor = 2;
  private const int Patch = 3;
  private const string PrereleaseTag = "feature/my-feature";
  private const int Counter = 32;
  private const string Metadata = "main";
  private const string Sha1 = "1234567890abcdef";

  [Test]
  public async Task Stable_version_only_is_reconstructed()
  {
    var version = SemanticVersionDescriptor.FromInformationalVersion($"{Major}.{Minor}.{Patch}");

    await Assert.That(version.Major).IsEqualTo(Major);
    await Assert.That(version.Minor).IsEqualTo(Minor);
    await Assert.That(version.Patch).IsEqualTo(Patch);
    await Assert.That(version.PreReleaseMetadata).IsNull();
    await Assert.That(version.BuildMetadata).IsNull();

    await Assert.That(version.ToString()).IsEqualTo($"{Major}.{Minor}.{Patch}");
  }

  [Test]
  public async Task Stable_version_is_reconstructed_without_build_metadata()
  {
    var version = SemanticVersionDescriptor.FromInformationalVersion($"{Major}.{Minor}.{Patch}+Branch.{Metadata}.Sha.{Sha1}");

    await Assert.That(version.Major).IsEqualTo(Major);
    await Assert.That(version.Minor).IsEqualTo(Minor);
    await Assert.That(version.Patch).IsEqualTo(Patch);
    await Assert.That(version.PreReleaseMetadata).IsNull();
    await Assert.That(version.BuildMetadata).IsNotNull();

    await Assert.That(version.ToString()).IsEqualTo($"{Major}.{Minor}.{Patch}");
  }

  [Test]
  public async Task Prerelease_version_is_reconstructed()
  {
    var version = SemanticVersionDescriptor.FromInformationalVersion(
        $"{Major}.{Minor}.{Patch}-{PrereleaseTag}.{Counter}");

    await Assert.That(version.Major).IsEqualTo(Major);
    await Assert.That(version.Minor).IsEqualTo(Minor);
    await Assert.That(version.Patch).IsEqualTo(Patch);
    await Assert.That(version.PreReleaseMetadata).IsNotNull();
    await Assert.That(version.BuildMetadata).IsNull();

    await Assert.That(version.ToString()).IsEqualTo($"{Major}.{Minor}.{Patch}-{PrereleaseTag}.{Counter}");
  }

  [Test]
  public async Task Full_version_is_reconstructed_without_build_metadata()
  {
    var version = SemanticVersionDescriptor.FromInformationalVersion(
        $"{Major}.{Minor}.{Patch}-{PrereleaseTag}.{Counter}+Branch.{Metadata}.Sha.{Sha1}");

    await Assert.That(version.Major).IsEqualTo(Major);
    await Assert.That(version.Minor).IsEqualTo(Minor);
    await Assert.That(version.Patch).IsEqualTo(Patch);
    await Assert.That(version.PreReleaseMetadata).IsNotNull();
    await Assert.That(version.PreReleaseMetadata!.Tag).IsEqualTo(PrereleaseTag);
    await Assert.That(version.PreReleaseMetadata.CommitCounter).IsEqualTo(Counter);
    await Assert.That(version.BuildMetadata).IsNotNull();
    await Assert.That(version.BuildMetadata!.Branch).IsEqualTo(Metadata);
    await Assert.That(version.BuildMetadata.Sha1).IsEqualTo(Sha1);

    await Assert.That(version.ToString()).IsEqualTo($"{Major}.{Minor}.{Patch}-{PrereleaseTag}.{Counter}");
  }
}
