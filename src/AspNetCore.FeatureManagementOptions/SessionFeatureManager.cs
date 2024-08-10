namespace Kritikos.AspNetCore.FeatureManagementOptions;

using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;

public class SessionFeatureManager(IHttpContextAccessor accessor)
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
          : (byte)0
      ]);

    return Task.CompletedTask;
  }

  /// <inheritdoc />
  public Task<bool?> GetAsync(string featureName)
  {
    var session = accessor.HttpContext?.Session;
    var sessionKey = $"feature_{featureName}";

    if ((session?.TryGetValue(sessionKey, out var enabledBytes) ?? false) && enabledBytes[0] == 1)
    {
      return Task.FromResult<bool?>(true);
    }

    return Task.FromResult<bool?>(false);
  }
}
