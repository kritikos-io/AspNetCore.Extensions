namespace Kritikos.HttpClient.Handlers.Tests;

using Kritikos.HttpClient.Handlers;

public class UserAgentProviderTests
{
  [Test]
  public async Task GetRandomUserAgent_returns_a_known_user_agent()
  {
    var provider = new UserAgentProvider(seed: 42);

    var agent = provider.GetRandomUserAgent();

    await Assert.That(agent).Contains("Mozilla/5.0");
  }

  [Test]
  public async Task GetRandomUserAgent_can_reach_every_agent()
  {
    var provider = new UserAgentProvider(seed: 12345);

    var seen = new HashSet<string>(StringComparer.Ordinal);
    for (var i = 0; i < 10_000; i++)
    {
      seen.Add(provider.GetRandomUserAgent());
    }

    // The previous implementation could only ever return the first agent per browser;
    // correct weighted selection must be able to reach all eight distinct agents.
    await Assert.That(seen.Count).IsEqualTo(8);
  }

  [Test]
  public async Task GetRandomUserAgent_is_deterministic_for_a_given_seed()
  {
    var first = new UserAgentProvider(seed: 7).GetRandomUserAgent();
    var second = new UserAgentProvider(seed: 7).GetRandomUserAgent();

    await Assert.That(first).IsEqualTo(second);
  }

  [Test]
  public async Task GetRandomUserAgent_uses_the_injected_agent_pool()
  {
    var options = new UserAgentProviderOptions { Agents = ["CustomAgent/1.0"] };
    var provider = new UserAgentProvider(new Random(1), options);

    await Assert.That(provider.GetRandomUserAgent()).IsEqualTo("CustomAgent/1.0");
  }

  [Test]
  public async Task An_empty_agent_pool_is_rejected()
  {
    await Assert.That(() => new UserAgentProvider(new UserAgentProviderOptions { Agents = [] }))
      .Throws<ArgumentException>();
  }
}
