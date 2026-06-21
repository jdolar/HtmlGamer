using Microsoft.Extensions.Logging;
namespace HtmlGamer.Core.Loggers.Services;
internal partial class Scrapper
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Debug,
        Message = "Opening {type}"
    )]
    internal static partial void OpenBrowser(
        ILogger logger,
        string type);
    
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "Navigating to: {url}"
    )]
    internal static partial void GoToPage(
        ILogger logger,
        string url);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Debug,
        Message = "Navigating to: {url} Index: {pageIndex}"
    )]
    internal static partial void GoToPage(
        ILogger logger,
        string url,
        int pageIndex);

    [LoggerMessage(
       EventId = 1003,
       Level = LogLevel.Debug,
       Message = "Processing page Index: {pageIndex} Url: {url}"
    )]
    internal static partial void GoToPage(
        ILogger logger,
        string url,
        string pageIndex);

    [LoggerMessage(
       EventId = 1003,
       Level = LogLevel.Debug,
       Message = "Processing page Index: {pageIndex} Url: {url}"
    )]
    internal static partial void ProcessPage(
       ILogger logger,
       int pageIndex,
       string url);

    [LoggerMessage(
       EventId = 1004,
       Level = LogLevel.Debug,
       Message = "Processing page Index: {pageIndex} Url: {url}"
    )]
    internal static partial void ProcessPage(
       ILogger logger,
       string pageIndex,
       string url);

    [LoggerMessage(
       EventId = 1004,
       Level = LogLevel.Debug,
       Message = "Filling field: {name} with value: {value}"
    )]
    internal static partial void FillField(
       ILogger logger,
       string name,
       string value);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Debug,
        Message = "Clicking on button: {name}"
    )]
    internal static partial void ClickButton(
        ILogger logger,
        string name);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Debug,
        Message = "Idle for {ms}ms"
    )]
    internal static partial void RandomDelay(
        ILogger logger,
        int ms);

    [LoggerMessage(
    EventId = 1007,
    Level = LogLevel.Debug,
    Message = "Saved page: {name} to: {path}"
)]
    internal static partial void SavePage(
    ILogger logger,
    string name,
    string path);
}