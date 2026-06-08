using Microsoft.Extensions.Logging;
namespace HtmlGamer.Core.Loggers;
internal partial class Scenario
{
    [LoggerMessage(
        EventId = 3000,
        Level = LogLevel.Debug,
        Message = "Executing step: {step}"
    )]
    internal static partial void Invoke(
        ILogger logger,
        string step);

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "Step: {step} completed"
)]
    internal static partial void Complete(
        ILogger logger,
        string step);
}
