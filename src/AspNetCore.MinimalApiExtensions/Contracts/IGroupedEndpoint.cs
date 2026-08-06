namespace Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Describes a Minimal API endpoint whose routes are mapped under a route <see cref="Group"/>. A relative group is
/// mapped under the <c>api/</c> convention; a group beginning with <c>/</c> is treated as absolute and mapped verbatim.
/// The default implementation creates the group, so implementations only map their routes onto it.
/// </summary>
public interface IGroupedEndpoint : IEndpoint
{
  /// <summary>
  /// Gets the route group the endpoint's routes are mapped under. A leading <c>/</c> maps the group verbatim; otherwise
  /// it is mapped under the <c>api/</c> convention.
  /// </summary>
  string Group { get; }

  /// <summary>
  /// Maps the endpoint's routes onto the created route group.
  /// </summary>
  /// <param name="group">The route group builder for the resolved <see cref="Group"/>.</param>
  void MapGroupedEndpoint(RouteGroupBuilder group);

  /// <inheritdoc />
  void IEndpoint.MapEndpoint(IEndpointRouteBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);
    var groupName = Group.StartsWith('/') ? Group : $"api/{Group}";
    var group = app.MapGroup(groupName);

    MapGroupedEndpoint(group);
  }
}
