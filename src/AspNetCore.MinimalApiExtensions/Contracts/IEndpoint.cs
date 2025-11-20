namespace Kritikos.AspNetCore.MinimalApiExtensions.Contracts;

using Microsoft.AspNetCore.Routing;

/// <summary>
/// Describes Minimal API endpoint classes.
/// </summary>
public interface IEndpoint
{
  /// <summary>
  /// Maps the endpoint to the provided <see cref="IEndpointRouteBuilder"/>.
  /// </summary>
  /// <param name="app">The app with the new endpoint mappings.</param>
  void MapEndpoint(IEndpointRouteBuilder app);
}
