namespace Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;

/// <summary>
/// An <see cref="ISessionManager"/> implementation that stores feature flag state in the ASP.NET Core session.
/// </summary>
/// <param name="accessor">The HTTP context accessor used to access session state.</param>
public sealed class SessionFeatureManager(IHttpContextAccessor accessor)
  : ISessionManager
{
  private readonly IHttpContextAccessor accessor = accessor;

  /// <inheritdoc />
  public Task SetAsync(string featureName, bool enabled)
  {
    var session = accessor.HttpContext?.Session;

    session?.Set(
      $"feature_{featureName}",
      [
        enabled
          ? (byte)1
          : (byte)0,
      ]);

    return Task.CompletedTask;
  }

  /// <inheritdoc />
  public Task<bool?> GetAsync(string featureName)
  {
    var session = accessor.HttpContext?.Session;
    var sessionKey = $"feature_{featureName}";

    return (session?.TryGetValue(sessionKey, out var enabledBytes) ?? false) && enabledBytes[0] == 1
      ? Task.FromResult<bool?>(true)
      : Task.FromResult<bool?>(false);
  }
}
