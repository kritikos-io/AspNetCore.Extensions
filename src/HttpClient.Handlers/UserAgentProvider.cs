// Copyright (c) Kritikos IO. All rights reserved.

namespace Kritikos.HttpClient.Handlers;

using Kritikos.HttpClient.Handlers.Contracts;

public class UserAgentProvider(Random random)
    : IUserAgentProvider
{
  private static readonly List<string> Agents =
  [
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/96.0.1054.62 Safari/537.36 Edg/96.0.1054.62",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:96.0) Gecko/20100101 Firefox/96.0",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36",
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.1 Safari/605.1.15",
    "Mozilla/5.0 (iPhone; CPU iPhone OS 15_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1",
    "Mozilla/5.0 (Linux; Android 11; SM-G998U1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Mobile Safari/537.36",
    "Mozilla/5.0 (Linux; Android 11; Pixel 4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Mobile Safari/537.36",
  ];

  private static readonly Dictionary<string, int> BrowserWeights = new()
  {
    { "Chrome", 30 },
    { "Firefox", 20 },
    { "Safari", 15 },
    { "Edge", 10 },
    { "Mobile Safari", 10 },
    { "Other", 5 },
  };

  public UserAgentProvider()
      : this(Random.Shared)
  {
  }

  /// <inheritdoc />
  public string GetRandomUserAgent()
  {
    var totalWeight = BrowserWeights.Values.Sum();
    var r = random.Next(totalWeight);
    var weight = 0;

    foreach (var browser in BrowserWeights.Keys)
    {
      foreach (var agent in Agents
                   .Where(x => x.Contains(browser, StringComparison.InvariantCultureIgnoreCase)))
      {
        weight += BrowserWeights[browser];
        if (weight > r)
        {
          return agent;
        }

        break;
      }
    }

    return Agents[^1];
  }
}
