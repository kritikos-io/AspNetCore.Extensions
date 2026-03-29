namespace Kritikos.Extensions.Options.Contracts;
#pragma warning disable SA1402

/// <summary>
/// Defines a contract for options classes that specify their configuration section location.
/// </summary>
public interface IOptionsDefinition
{
  /// <summary>
  /// Gets the name of the section containing the options in the configuration.
  /// </summary>
  static abstract string Location { get; }
}

/// <summary>
/// Defines a contract for named options classes that specify multiple named configuration section locations.
/// </summary>
public interface INamedOptionsDefinition
{
  /// <summary>
  /// Gets a dictionary of (Name, Location) pairings for named options.
  /// </summary>
  /// <remarks>
  /// Location should correspond to a configuration section.
  /// </remarks>
  static abstract Dictionary<string, string> NamedLocations { get; }
}
