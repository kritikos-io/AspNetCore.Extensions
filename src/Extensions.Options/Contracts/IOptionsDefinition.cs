namespace Kritikos.Extensions.Options.Contracts;
#pragma warning disable SA1402

public interface IOptionsDefinition
{
  /// <summary>
  /// Gets the name of the section containing the options in the configuration.
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
