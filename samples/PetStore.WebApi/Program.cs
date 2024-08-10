using Asp.Versioning.Builder;

using Kritikos.PetStore.WebApi;

var builder = WebApplication.CreateBuilder(args);

var app = builder.UseStartup<Startup>();

await app.RunAsync();

public sealed partial class Program
{
  internal static ApiVersionSet VersionSet { get; set; } = default!;
}
