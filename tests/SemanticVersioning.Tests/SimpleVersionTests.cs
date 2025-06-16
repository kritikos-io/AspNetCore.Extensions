namespace Kritikos.SemanticVersioning.Tests;

using Xunit;

public class SimpleVersionTests
{
  [Fact]
  public void Stable_version_is_parsed()
  {
    const string version = "1.0.3";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    Assert.Equal(1, semanticVersion.Major);
    Assert.Equal(0, semanticVersion.Minor);
    Assert.Equal(3, semanticVersion.Patch);
    Assert.Null(semanticVersion.PreReleaseMetadata);
    Assert.Null(semanticVersion.BuildMetadata);
  }

  [Fact]
  public void PreRelease_version_is_parsed()
  {
    const string version = "1.0.3-alpha.1";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    Assert.Equal(1, semanticVersion.Major);
    Assert.Equal(0, semanticVersion.Minor);
    Assert.Equal(3, semanticVersion.Patch);
    Assert.NotNull(semanticVersion.PreReleaseMetadata);
    Assert.Equal("alpha", semanticVersion.PreReleaseMetadata?.Tag);
    Assert.Equal(1, semanticVersion.PreReleaseMetadata?.CommitCounter);

    Assert.Null(semanticVersion.BuildMetadata);
  }

  [Fact]
  public void Build_metadata_version_is_parsed()
  {
    const string version = "1.0.3+Branch.main.Sha.186bfb5a230fb8821286f99e8b28bd4d32433c91";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    Assert.Equal(1, semanticVersion.Major);
    Assert.Equal(0, semanticVersion.Minor);
    Assert.Equal(3, semanticVersion.Patch);
    Assert.Null(semanticVersion.PreReleaseMetadata);
    Assert.NotNull(semanticVersion.BuildMetadata);
    Assert.Equal("main", semanticVersion.BuildMetadata.Branch);
    Assert.Equal("186bfb5a230fb8821286f99e8b28bd4d32433c91", semanticVersion.BuildMetadata.Sha1);
  }

  [Fact]
  public void Full_version_is_parsed()
  {
    const string version = "1.2.1-rc.72+Branch.main.Sha.186bfb5a230fb8821286f99e8b28bd4d32433c91";

    var semanticVersion = SemanticVersionDescriptor.FromInformationalVersion(version);

    Assert.Equal(1, semanticVersion.Major);
    Assert.Equal(2, semanticVersion.Minor);
    Assert.Equal(1, semanticVersion.Patch);
    Assert.Equal("rc", semanticVersion.PreReleaseMetadata?.Tag);
    Assert.Equal(72, semanticVersion.PreReleaseMetadata?.CommitCounter);
    Assert.Equal("main", semanticVersion.BuildMetadata?.Branch);
    Assert.Equal("186bfb5a230fb8821286f99e8b28bd4d32433c91", semanticVersion.BuildMetadata?.Sha1);
  }
}
