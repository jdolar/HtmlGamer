using HtmlGamer.Core;
using HtmlGamer.Core.Data;
using Microsoft.Extensions.Hosting;
bool encryptConstants = true;
IHost host = await Configure.BuildHost(args, encryptConstants)
                            .Validate();

var runner = host.GetRunner();
await runner.RunScenarios();

runner.Close();