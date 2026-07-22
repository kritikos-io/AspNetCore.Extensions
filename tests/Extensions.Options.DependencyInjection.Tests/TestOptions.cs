namespace Kritikos.Extensions.Options.DependencyInjection.Tests;
#pragma warning disable SA1402 // File may only contain a single type - options fixtures are grouped intentionally.

using System.ComponentModel.DataAnnotations;

using Kritikos.Extensions.Options.Contracts;

public sealed class SampleOptions : IOptionsDefinition
{
  public static string Location => "Sample";

  [Required]
  public string Name { get; set; } = string.Empty;

  [Range(1, 100)]
  public int Count { get; set; }
}

public sealed class SecondSampleOptions : IOptionsDefinition
{
  public static string Location => "Second";

  public string Value { get; set; } = string.Empty;
}

public abstract class AbstractSampleOptions : IOptionsDefinition
{
  public static string Location => "Abstract";
}

public sealed class NamedSampleOptions : INamedOptionsDefinition
{
  public static Dictionary<string, string> NamedLocations => new(StringComparer.Ordinal)
  {
    ["primary"] = "Named:Primary",
    ["secondary"] = "Named:Secondary",
  };

  public string Value { get; set; } = string.Empty;
}
