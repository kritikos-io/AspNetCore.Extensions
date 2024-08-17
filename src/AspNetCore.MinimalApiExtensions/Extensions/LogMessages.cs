namespace Kritikos.AspNetCore.MinimalApiExtensions.Extensions;

public static partial class LogMessages
{
  private const string UnhandledExceptionMessage = "An unhandled exception occurred";

  private const string PeriodicBackgroundServiceTriggeredMessage =
      "Periodic background service {Service} was triggered";

  private const string PeriodicBackgroundServiceSleepingMessage =
      "Periodic background service {Service} going to sleep";

  private const string EntityNotFoundMessage = "Requested entity {Entity} with id {Id} was not found";
}

public static partial class LogMessages
{
  [LoggerMessage(LogLevel.Critical, UnhandledExceptionMessage)]
  public static partial void LogUnhandledException(this ILogger logger, Exception e);

  [LoggerMessage(LogLevel.Information, PeriodicBackgroundServiceTriggeredMessage)]
  public static partial void LogPeriodicBackgroundServiceTriggered(this ILogger logger, string service);

  [LoggerMessage(LogLevel.Debug, PeriodicBackgroundServiceSleepingMessage)]
  public static partial void LogPeriodicBackgroundServiceSleeping(this ILogger logger, string service);

  [LoggerMessage(LogLevel.Error, EntityNotFoundMessage)]
  public static partial void LogEntityNotFound(this ILogger logger, string entity, string id);
}
