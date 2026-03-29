namespace Kritikos.AspNetCore.VersioningOptions.Contracts;
#pragma warning disable SA1402

using Asp.Versioning;
using Asp.Versioning.Builder;

/// <summary>
/// Provides the <see cref="ApiVersionModel"/> describing supported and deprecated API versions.
/// </summary>
public interface IApiVersionModelProvider
{
  /// <summary>
  /// Gets the <see cref="ApiVersionModel"/> containing supported and deprecated API versions.
  /// </summary>
  static abstract ApiVersionModel VersionModel { get; }
}

/// <summary>
/// Provides an <see cref="ApiVersionSet"/> for building versioned endpoint groups.
/// </summary>
public interface IApiVersionSetProvider
{
  /// <summary>
  /// Gets or sets the <see cref="ApiVersionSet"/> used by versioned endpoints.
  /// </summary>
  static abstract ApiVersionSet VersionSet { get; set; }
}
