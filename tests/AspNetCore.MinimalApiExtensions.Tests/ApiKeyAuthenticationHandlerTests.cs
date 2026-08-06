namespace Kritikos.AspNetCore.MinimalApiExtensions.Tests;

using System.Security.Claims;
using System.Text.Encodings.Web;

using Kritikos.AspNetCore.MinimalApiExtensions.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;

public class ApiKeyAuthenticationHandlerTests
{
  [Test]
  public async Task Missing_header_returns_no_result()
  {
    var result = await AuthenticateAsync(Substitute.For<IApiKeyValidator>());

    await Assert.That(result.None).IsTrue();
  }

  [Test]
  public async Task Empty_header_returns_no_result()
  {
    var result = await AuthenticateAsync(
      Substitute.For<IApiKeyValidator>(),
      context => context.Request.Headers[ApiKeyDefaults.HeaderName] = string.Empty);

    await Assert.That(result.None).IsTrue();
  }

  [Test]
  public async Task Invalid_key_fails()
  {
    var validator = Substitute.For<IApiKeyValidator>();
    validator.ValidateAsync("bad", Arg.Any<CancellationToken>())
      .Returns(new ValueTask<ClaimsPrincipal?>((ClaimsPrincipal?)null));

    var result = await AuthenticateAsync(
      validator,
      context => context.Request.Headers[ApiKeyDefaults.HeaderName] = "bad");

    await Assert.That(result.Succeeded).IsFalse();
    await Assert.That(result.Failure).IsNotNull();
  }

  [Test]
  public async Task Valid_key_succeeds_with_principal()
  {
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
      [new Claim(ClaimTypes.Name, "svc")], ApiKeyDefaults.AuthenticationScheme));
    var validator = Substitute.For<IApiKeyValidator>();
    validator.ValidateAsync("good", Arg.Any<CancellationToken>())
      .Returns(new ValueTask<ClaimsPrincipal?>(principal));

    var result = await AuthenticateAsync(
      validator,
      context => context.Request.Headers[ApiKeyDefaults.HeaderName] = "good");

    await Assert.That(result.Succeeded).IsTrue();
    await Assert.That(result.Principal!.Identity!.Name).IsEqualTo("svc");
  }

  private static async Task<AuthenticateResult> AuthenticateAsync(
    IApiKeyValidator validator,
    Action<DefaultHttpContext>? configure = null)
  {
    var monitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
    monitor.Get(Arg.Any<string?>()).Returns(new ApiKeyAuthenticationOptions());

    var handler = new ApiKeyAuthenticationHandler(
      monitor, NullLoggerFactory.Instance, UrlEncoder.Default, validator);

    var context = new DefaultHttpContext();
    configure?.Invoke(context);

    await handler.InitializeAsync(
      new AuthenticationScheme(ApiKeyDefaults.AuthenticationScheme, null, typeof(ApiKeyAuthenticationHandler)),
      context);

    return await handler.AuthenticateAsync();
  }
}
