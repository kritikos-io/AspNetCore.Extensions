#pragma warning disable CA5394 // Do not use insecure randomness

namespace Kritikos.HttpClient.Handlers;

using Kritikos.HttpClient.Handlers.Contracts;

/// <summary>
/// Provides random user agent strings with weighted browser selection.
/// </summary>
public sealed class UserAgentProvider : IUserAgentProvider
{
  private readonly Random random;
  private readonly (string Agent, int Weight)[] weighted;
  private readonly int totalWeight;

  /// <summary>Initializes a new instance of the <see cref="UserAgentProvider"/> class.</summary>
  /// <param name="random">The random number generator used for weighted selection.</param>
  /// <param name="options">The agent pool and weights to use, or <see langword="null"/> for the defaults.</param>
  /// <exception cref="ArgumentNullException"><paramref name="random"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException"><paramref name="options"/> configures an empty agent pool.</exception>
  public UserAgentProvider(Random random, UserAgentProviderOptions? options = null)
  {
    ArgumentNullException.ThrowIfNull(random);
    var resolved = options ?? new UserAgentProviderOptions();
    if (resolved.Agents.Count == 0)
    {
      throw new ArgumentException("At least one user agent must be configured.", nameof(options));
    }

    this.random = random;
    weighted = [.. resolved.Agents.Select(agent => (Agent: agent, Weight: GetWeight(agent, resolved.BrowserWeights)))];
    totalWeight = weighted.Sum(static pair => pair.Weight);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="UserAgentProvider"/> class using <see cref="Random.Shared"/>.
  /// </summary>
  /// <param name="options">The agent pool and weights to use.</param>
  public UserAgentProvider(UserAgentProviderOptions options)
    : this(Random.Shared, options)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="UserAgentProvider"/> class using <see cref="Random.Shared"/>.
  /// </summary>
  public UserAgentProvider()
    : this(Random.Shared)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="UserAgentProvider"/> class with a specific seed for reproducible results.
  /// </summary>
  /// <param name="seed">The seed for the random number generator.</param>
  public UserAgentProvider(int seed)
    : this(new Random(seed))
  {
  }

  /// <inheritdoc />
  public string GetRandomUserAgent()
  {
    var target = random.Next(totalWeight);

    var cumulative = 0;
    foreach (var (agent, weight) in weighted)
    {
      cumulative += weight;
      if (target < cumulative)
      {
        return agent;
      }
    }

    return weighted[^1].Agent;
  }

  private static int GetWeight(string agent, IReadOnlyDictionary<string, int> weights)
  {
    var category = GetCategory(agent);
    return weights.TryGetValue(category, out var weight) ? weight : 1;
  }

  private static string GetCategory(string agent)
  {
    if (agent.Contains("Edg", StringComparison.InvariantCultureIgnoreCase))
    {
      return "Edge";
    }

    if (agent.Contains("Firefox", StringComparison.InvariantCultureIgnoreCase))
    {
      return "Firefox";
    }

    if (agent.Contains("Mobile", StringComparison.InvariantCultureIgnoreCase))
    {
      return "Mobile Safari";
    }

    if (agent.Contains("Chrome", StringComparison.InvariantCultureIgnoreCase))
    {
      return "Chrome";
    }

    if (agent.Contains("Safari", StringComparison.InvariantCultureIgnoreCase))
    {
      return "Safari";
    }

    return "Other";
  }
}
