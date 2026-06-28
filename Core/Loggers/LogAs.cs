using Microsoft.Extensions.Logging;
namespace HtmlGamer.Core.Loggers;
internal partial class LogAs
{
    [LoggerMessage(
    EventId = 1000,
    Level = LogLevel.Debug,
    Message = "Initialized"
    )]
    internal static partial void Init(
    ILogger logger);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "{message}"
    )]
    internal static partial void Debug(
        ILogger logger,
        string message);
    
    [LoggerMessage(
      EventId = 1001,
      Level = LogLevel.Debug,
      Message = "{message} {details}"
    )]
    internal static partial void Debug(
      ILogger logger,
      string message,
      string details);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "{message}"
    )]
    internal static partial void Error(
        ILogger logger,
        string message,
        Exception ex);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "{message}"
    )]
    internal static partial void Error(
        ILogger logger,
        string message);

    [LoggerMessage(
        EventId = 1002,
          Level = LogLevel.Error,
          Message = "{message}: {details}"
    )]
    internal static partial void Error(
        ILogger logger,
        string message,
        string details);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "{message}"
    )]
    internal static partial void Warning(
        ILogger logger,
        string message);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "{message}: {details}"
    )]
    internal static partial void Warning(
    ILogger logger,
    string message,
    string details);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "{message}"
    )]
    internal static partial void Info(
    ILogger logger,
    string message);
}
