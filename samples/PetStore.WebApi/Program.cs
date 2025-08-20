using Asp.Versioning;
using Asp.Versioning.Builder;

using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.AspNetCore.VersioningOptions.Contracts;
using Kritikos.PetStore.WebApi;

var builder = WebApplication.CreateBuilder(args);

var app = builder.UseStartup<Startup>();

await app.RunAsync();

public sealed partial class Program : IApiVersionModelProvider, IApiVersionSetProvider
{
  public static ApiVersionModel VersionModel { get; } = new(
    [new ApiVersion(1), new ApiVersion(0), new ApiVersion(3)],
    [new ApiVersion(2)]);

  public static ApiVersionSet VersionSet { get; set; } = default!;
}
