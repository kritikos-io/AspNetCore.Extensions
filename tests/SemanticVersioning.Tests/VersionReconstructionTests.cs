namespace Kritikos.SemanticVersioning.Tests;

using Xunit;

public class VersionReconstructionTests
{
  private const int Major = 1;
  private const int Minor = 2;
  private const int Patch = 3;
  private const string PrereleaseTag = "feature/my-feature";
  private const int Counter = 32;
  private const string Metadata = "main";
  private const string Sha1 = "1234567890abcdef";

  [Fact]
  public void Stable_version_only_is_reconstructed()
  {
    var version = SemanticVersionDescriptor.FromInformationalVersion($"{Major}.{Minor}.{Patch}");

    Assert.Equal(Major, version.Major);
    Assert.Equal(Minor, version.Minor);
    Assert.Equal(Patch, version.Patch);
    Assert.Null(version.PreReleaseMetadata);
    Assert.Null(version.BuildMetadata);

    Assert.Equal($"{Major}.{Minor}.{Patch}", version.ToString());
  }

  [Fact]
  public void Stable_version_is_reconstructed_without_build_metadata()
  {
    var version = SemanticVersionDescriptor.FromInformationalVersion($"{Major}.{Minor}.{Patch}+Branch.{Metadata}.Sha.{Sha1}");

    Assert.Equal(Major, version.Major);
    Assert.Equal(Minor, version.Minor);
    Assert.Equal(Patch, version.Patch);
    Assert.Null(version.PreReleaseMetadata);
    Assert.NotNull(version.BuildMetadata);

    Assert.Equal($"{Major}.{Minor}.{Patch}", version.ToString());
  }

  [Fact]
  public void Prerelease_version_is_reconstructed()
  {
    var version = SemanticVersionDescriptor.FromInformationalVersion(
        $"{Major}.{Minor}.{Patch}-{PrereleaseTag}.{Counter}");

    Assert.Equal(Major, version.Major);
    Assert.Equal(Minor, version.Minor);
    Assert.Equal(Patch, version.Patch);
    Assert.NotNull(version.PreReleaseMetadata);
    Assert.Null(version.BuildMetadata);

    Assert.Equal($"{Major}.{Minor}.{Patch}-{PrereleaseTag}.{Counter}", version.ToString());
  }

  [Fact]
  public void Full_version_is_reconstructed_without_build_metadata()
  {
    var version = SemanticVersionDescriptor.FromInformationalVersion(
        $"{Major}.{Minor}.{Patch}-{PrereleaseTag}.{Counter}+Branch.{Metadata}.Sha.{Sha1}");

    Assert.Equal(Major, version.Major);
    Assert.Equal(Minor, version.Minor);
    Assert.Equal(Patch, version.Patch);
    Assert.NotNull(version.PreReleaseMetadata);
    Assert.Equal(PrereleaseTag, version.PreReleaseMetadata.Tag);
    Assert.Equal(Counter, version.PreReleaseMetadata.CommitCounter);
    Assert.NotNull(version.BuildMetadata);
    Assert.Equal(Metadata, version.BuildMetadata.Branch);
    Assert.Equal(Sha1, version.BuildMetadata.Sha1);

    Assert.Equal($"{Major}.{Minor}.{Patch}-{PrereleaseTag}.{Counter}", version.ToString());
  }
}
