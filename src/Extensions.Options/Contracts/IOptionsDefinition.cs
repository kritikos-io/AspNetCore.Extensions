namespace Kritikos.Extensions.Options.Contracts;

public interface IOptionsDefinition
{
  /// <summary>
  /// The location of the options in the configuration.
  /// </summary>
  static abstract string Location { get; }
}

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
