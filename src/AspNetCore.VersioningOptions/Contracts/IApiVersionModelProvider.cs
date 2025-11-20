namespace Kritikos.AspNetCore.VersioningOptions.Contracts;
#pragma warning disable SA1402

using Asp.Versioning;
using Asp.Versioning.Builder;

public interface IApiVersionModelProvider
{
  static abstract ApiVersionModel VersionModel { get; }
}

public interface IApiVersionSetProvider
{
  static abstract ApiVersionSet VersionSet { get; set; }
}
