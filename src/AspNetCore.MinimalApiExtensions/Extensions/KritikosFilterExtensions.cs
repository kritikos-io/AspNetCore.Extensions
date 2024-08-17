// ReSharper disable once CheckNamespace : Recommendation by Microsoft for dependency injection extension methods

namespace Microsoft.Extensions.DependencyInjection;

using System.Reflection;

public static class EndpointFilterExtensions
{
  public static TBuilder WithParameterValidation<TBuilder>(this TBuilder endpoint, int statusCode = StatusCodes.Status400BadRequest)
      where TBuilder : IEndpointConventionBuilder
  {
    ArgumentNullException.ThrowIfNull(endpoint);

    endpoint.Add(
        builder =>
        {
          var loggerFactory = builder.ApplicationServices.GetRequiredService<ILoggerFactory>();
          var logger = loggerFactory.CreateLogger("ParameterValidationFilter");

          var methodInfo = builder.Metadata.OfType<MethodInfo>().FirstOrDefault();
        });

    return endpoint;
  }
}
