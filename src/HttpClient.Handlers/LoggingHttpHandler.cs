namespace Kritikos.HttpClient.Handlers;

using Microsoft.Extensions.Logging;

public class LoggingHttpHandler(ILogger<LoggingHttpHandler> logger) : DelegatingHandler
{
}
