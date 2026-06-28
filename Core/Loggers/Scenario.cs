using Microsoft.Extensions.Logging;
namespace HtmlGamer.Core.Loggers;
internal partial class Scenario
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Debug,
        Message = "[{account}] Executing scenario: {scenario}"
    )]
    internal static partial void InvokeScenario(
        ILogger logger,
        string account,
        string scenario);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "[{account}] {scenario} completed"
    )]
    internal static partial void CompleteScenario(
        ILogger logger,
        string account,
        string scenario);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "Executing step: {step}"
    )]
    internal static partial void InvokeStep(
        ILogger logger,
        string step);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Debug,
        Message = "Step: {step} completed"
    )]
    internal static partial void CompleteStep(
        ILogger logger,
        string step);
}
