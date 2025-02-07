namespace Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

public interface IOptionsDefinition
{
  /// <summary>
  /// The location of the options in the configuration.
  /// </summary>
  static abstract string Location { get; }
}
