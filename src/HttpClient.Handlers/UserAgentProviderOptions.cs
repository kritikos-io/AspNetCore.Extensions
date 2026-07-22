namespace Kritikos.HttpClient.Handlers;

/// <summary>
/// Configuration for <see cref="UserAgentProvider"/>: the pool of user agent strings to draw from and the
/// per-browser weights that bias the random selection. The defaults ship a current, representative set; supply
/// your own (optionally extending <see cref="DefaultAgents"/>) to keep the pool fresh or to target specific browsers.
/// </summary>
public sealed class UserAgentProviderOptions
{
  /// <summary>Gets the built-in, representative pool of user agent strings.</summary>
  public static IReadOnlyList<string> DefaultAgents { get; } =
  [
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36 Edg/134.0.3124.85",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:136.0) Gecko/20100101 Firefox/136.0",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_7_4) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.3 Safari/605.1.15",
    "Mozilla/5.0 (iPhone; CPU iPhone OS 18_3_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.3.1 Mobile/15E148 Safari/604.1",
    "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Mobile Safari/537.36",
    "Mozilla/5.0 (Android 15; Mobile; rv:136.0) Gecko/136.0 Firefox/136.0",
  ];

  /// <summary>Gets the built-in per-browser selection weights, keyed by the category names used to classify agents.</summary>
  public static IReadOnlyDictionary<string, int> DefaultBrowserWeights { get; } =
    new Dictionary<string, int>(StringComparer.Ordinal)
    {
      ["Chrome"] = 30,
      ["Firefox"] = 20,
      ["Safari"] = 15,
      ["Edge"] = 10,
      ["Mobile Safari"] = 10,
      ["Other"] = 5,
    };

  /// <summary>Gets the pool of user agent strings to select from. Defaults to <see cref="DefaultAgents"/>.</summary>
  public IReadOnlyList<string> Agents { get; init; } = DefaultAgents;

  /// <summary>Gets the per-browser selection weights. Defaults to <see cref="DefaultBrowserWeights"/>.</summary>
  public IReadOnlyDictionary<string, int> BrowserWeights { get; init; } = DefaultBrowserWeights;
}
