using HtmlGamer.Core;
using HtmlGamer.Core.Services;
using Microsoft.Extensions.DependencyInjection;

bool encryptConstants = true;

var host = Configure.BuildHost(args, encryptConstants);

var runner = host.Services.GetRequiredService<Runner>();
await runner.Run();

runner.Close();
;