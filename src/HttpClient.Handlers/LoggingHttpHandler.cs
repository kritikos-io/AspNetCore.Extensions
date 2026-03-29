namespace Kritikos.HttpClient.Handlers;

using System.Net;

using Microsoft.Extensions.Logging;

/// <summary>
/// A delegating handler that logs HTTP request start and completion details at the debug level.
/// </summary>
/// <param name="logger">The logger instance used for structured logging.</param>
public partial class LoggingHttpHandler(ILogger<LoggingHttpHandler> logger) : DelegatingHandler
{
  private ILogger logger = logger;

  /// <inheritdoc />
  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(request);

    if (!logger.IsEnabled(LogLevel.Debug))
    {
      return await base.SendAsync(request, cancellationToken);
    }

    LogHttpRequestStarting(logger, request.Method, request.RequestUri, DateTime.UtcNow);

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var response = await base.SendAsync(request, cancellationToken);
    stopwatch.Stop();

    LogHttpRequestCompleted(logger, request.Method, request.RequestUri, stopwatch.Elapsed, response.StatusCode);

    return response;
  }

  [LoggerMessage(LogLevel.Debug, "HttpRequest {HttpMethod} {RequestUri} starting at {Now}")]
  private static partial void
    LogHttpRequestStarting(ILogger logger, HttpMethod httpMethod, Uri? requestUri, DateTime now);

  [LoggerMessage(LogLevel.Debug,
    "HttpRequest {HttpMethod} {RequestUri} completed in {Elapsed} with status code {StatusCode}")]
  private static partial void LogHttpRequestCompleted(ILogger logger, HttpMethod httpMethod, Uri? requestUri,
    TimeSpan Elapsed, HttpStatusCode statusCode);
}
