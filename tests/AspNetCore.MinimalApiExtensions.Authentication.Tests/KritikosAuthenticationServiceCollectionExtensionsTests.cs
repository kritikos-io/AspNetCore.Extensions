namespace Kritikos.AspNetCore.MinimalApiExtensions.Authentication.Tests;

using Kritikos.AspNetCore.MinimalApiExtensions.WellKnown;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class KritikosAuthenticationServiceCollectionExtensionsTests
{
  [Test]
  public async Task AddOidcProtectedResource_null_services_throws()
    => await Assert.That(() => ((IServiceCollection)null!).AddOidcProtectedResource())
      .Throws<ArgumentNullException>();

  [Test]
  public async Task AddOidcProtectedResource_adds_bearer_authority_to_authorization_servers()
  {
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
    services.AddAuthentication().AddJwtBearer(options => options.Authority = "https://demo.identity");

    services.AddOidcProtectedResource(static options => options.ScopesSupported.Add("openid"));

    await using var provider = services.BuildServiceProvider();
    var resource = provider.GetRequiredService<IOptions<OAuthProtectedResourceOptions>>().Value;

    await Assert.That(resource.AuthorizationServers).Contains(new Uri("https://demo.identity"));
    await Assert.That(resource.ScopesSupported).Contains("openid");
  }

  [Test]
  public async Task AddOidcProtectedResource_ignores_missing_authority()
  {
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
    services.AddAuthentication().AddJwtBearer();

    services.AddOidcProtectedResource();

    await using var provider = services.BuildServiceProvider();
    var resource = provider.GetRequiredService<IOptions<OAuthProtectedResourceOptions>>().Value;

    await Assert.That(resource.AuthorizationServers).IsEmpty();
  }
}
