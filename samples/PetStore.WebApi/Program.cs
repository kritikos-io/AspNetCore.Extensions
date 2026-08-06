using Kritikos.AspNetCore.MinimalApiExtensions.Extensions;
using Kritikos.PetStore.WebApi;

var builder = WebApplication.CreateBuilder(args);

var app = builder.UseStartup<Startup>();

await app.RunAsync();

public sealed partial class Program;
