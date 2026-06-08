using HtmlGamer.Core;
using HtmlGamer.Core.Data;
using HtmlGamer.Core.Services;
using Microsoft.Extensions.DependencyInjection;
bool encryptConstants = true;

var host = Configure.BuildHost(args, encryptConstants)
    .ValidateConfigs()
    .ValidatePlaywright();

var runner = host.Services.GetRequiredService<Runner>();
await runner.Run();

runner.Close();
;