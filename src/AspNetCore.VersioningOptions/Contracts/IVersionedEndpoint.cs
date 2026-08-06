namespace Kritikos.AspNetCore.VersioningOptions.Contracts;

using Asp.Versioning;
using Asp.Versioning.Builder;

using Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Describes a Minimal API endpoint that targets a single API <see cref="Version"/>. A relative
/// <see cref="IGroupedEndpoint.Group"/> is mapped under the <c>api/v{apiVersion:apiVersion}/</c> convention; a group
/// beginning with <c>/</c> is mapped verbatim and owns its version segment. The default implementation resolves the
/// shared <see cref="ApiVersionSet"/> from the application's services and associates the group with both the set and
/// the endpoint's <see cref="Version"/>, which must be one of the versions configured on the set.
/// </summary>
public interface IVersionedEndpoint : IGroupedEndpoint
{
  /// <summary>
  /// Gets the API version this endpoint is mapped to.
  /// </summary>
  ApiVersion Version { get; }

  /// <inheritdoc />
  void IEndpoint.MapEndpoint(IEndpointRouteBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    var versionSet = app.ServiceProvider.GetRequiredService<ApiVersionSet>();
    var model = app.ServiceProvider.GetRequiredService<ApiVersionModel>();

    if (!model.SupportedApiVersions.Contains(Version) && !model.DeprecatedApiVersions.Contains(Version))
    {
      throw new InvalidOperationException(
        $"'{GetType().Name}' declares API version '{Version}', which is not configured in the ApiVersionSet.");
    }

    var route = Group.StartsWith('/') ? Group : $"api/v{{apiVersion:apiVersion}}/{Group}";

    var group = app.MapGroup(route)
      .WithApiVersionSet(versionSet)
      .MapToApiVersion(Version);

    MapGroupedEndpoint(group);
  }
}
