namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

using Microsoft.Extensions.Logging;

/// <summary>
/// Log message template constants for structured logging.
/// </summary>
public static partial class LogMessages
{
  private const string UnhandledExceptionMessage = "An unhandled exception occurred";

  private const string PeriodicBackgroundServiceTriggeredMessage =
      "Periodic background service {Service} was triggered";

  private const string PeriodicBackgroundServiceTriggeredManuallyMessage =
          "Periodic background service {Service} was triggered by manual request";

  private const string PeriodicBackgroundServiceSleepingMessage =
      "Periodic background service {Service} going to sleep";

  private const string EntityNotFoundMessage = "Requested entity {Entity} with id {Id} was not found";

  private const string HeartbeatSourceCancelledMessage =
    "Heartbeat watchdog cancelled stale source {Source}";

  private const string HeartbeatSourceCancelFailedMessage =
    "Heartbeat watchdog failed to cancel stale source {Source}";
}

/// <summary>
/// High-performance source-generated log message methods.
/// </summary>
public static partial class LogMessages
{
  /// <summary>
  /// Logs an unhandled exception at the critical level.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="e">The unhandled exception.</param>
  [LoggerMessage(LogLevel.Critical, UnhandledExceptionMessage)]
  public static partial void LogUnhandledException(this ILogger logger, Exception e);

  /// <summary>
  /// Logs that a periodic background service was triggered manually.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="service">The name of the background service.</param>
  [LoggerMessage(LogLevel.Information, PeriodicBackgroundServiceTriggeredManuallyMessage)]
  public static partial void LogPeriodicBackgroundServiceTriggeredManually(this ILogger logger, string service);

  /// <summary>
  /// Logs that a periodic background service was triggered on schedule.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="service">The name of the background service.</param>
  [LoggerMessage(LogLevel.Debug, PeriodicBackgroundServiceTriggeredMessage)]
  public static partial void LogPeriodicBackgroundServiceTriggered(this ILogger logger, string service);

  /// <summary>
  /// Logs that a periodic background service is going to sleep.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="service">The name of the background service.</param>
  [LoggerMessage(LogLevel.Debug, PeriodicBackgroundServiceSleepingMessage)]
  public static partial void LogPeriodicBackgroundServiceSleeping(this ILogger logger, string service);

  /// <summary>
  /// Logs that a requested entity was not found.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="entity">The entity type name.</param>
  /// <param name="id">The entity identifier.</param>
  [LoggerMessage(LogLevel.Error, EntityNotFoundMessage)]
  public static partial void LogEntityNotFound(this ILogger logger, string entity, string id);

  /// <summary>
  /// Logs that the heartbeat watchdog cancelled a stale source.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="source">The name of the cancelled heartbeat source.</param>
  [LoggerMessage(LogLevel.Warning, HeartbeatSourceCancelledMessage)]
  public static partial void LogHeartbeatSourceCancelled(this ILogger logger, string source);

  /// <summary>
  /// Logs that the heartbeat watchdog failed to cancel a stale source because its cancellation threw.
  /// </summary>
  /// <param name="logger">The logger instance.</param>
  /// <param name="source">The name of the heartbeat source whose cancellation threw.</param>
  /// <param name="e">The exception raised while cancelling the source.</param>
  [LoggerMessage(LogLevel.Error, HeartbeatSourceCancelFailedMessage)]
  public static partial void LogHeartbeatSourceCancelFailed(this ILogger logger, string source, Exception e);
}
