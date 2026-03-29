namespace Kritikos.AspNetCore.MinimalApiExtensions.Middleware;

using Kritikos.AspNetCore.MinimalApiExtensions.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Middleware that ensures each HTTP request has a correlation identifier header for distributed tracing.
/// </summary>
public class CorrelationHeaderMiddleware : IMiddleware
{
  private readonly CorrelationHeaderOptions options;
  private readonly ILogger logger;

  /// <summary>
  /// Initializes a new instance of the <see cref="CorrelationHeaderMiddleware"/> class.
  /// </summary>
  /// <param name="options">The correlation header options.</param>
  /// <param name="logger">The logger instance.</param>
  public CorrelationHeaderMiddleware(IOptions<CorrelationHeaderOptions> options, ILogger<CorrelationHeaderMiddleware> logger)
  {
    ArgumentNullException.ThrowIfNull(options);

    this.options = options.Value;
    this.logger = logger;
  }

  /// <inheritdoc />
  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(next);

    if (!context.Request.Headers.TryGetValue(options.Header, out var correlationId))
    {
      correlationId = Guid.NewGuid().ToString();
      context.Request.Headers[options.Header] = correlationId;
    }

    if (options.IncludeInResponse)
    {
      context.Response.OnStarting(
          () =>
          {
            if (!context.Response.Headers.ContainsKey(options.Header))
            {
              context.Response.Headers[options.Header] = correlationId;
            }

            return Task.CompletedTask;
          });
    }

    using (logger.BeginScope(new Dictionary<string, object> { { options.Header, correlationId } }))
    {
      await next(context);
    }
  }
}
