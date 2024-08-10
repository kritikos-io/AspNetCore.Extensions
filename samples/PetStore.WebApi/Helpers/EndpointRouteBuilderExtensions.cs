namespace Kritikos.PetStore.WebApi.Helpers;

using System.Reflection;

using Asp.Versioning;
using Asp.Versioning.Builder;

public static class EndpointRouteBuilderExtensions
{
  public static ApiVersionSet ApiVersionSetBuilder(Assembly assembly)
    => new ApiVersionSetBuilder(Assembly.GetExecutingAssembly().GetName().Name)
        .HasApiVersion(new ApiVersion(1))
        .HasApiVersion(new ApiVersion(2))
        .Build();
}
