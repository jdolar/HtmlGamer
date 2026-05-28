using HtmlGamer.Core;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.OutPut;
using HtmlGamer.Core.Services;
using Microsoft.Extensions.DependencyInjection;

bool encryptConstants = false;

var host = Configure.BuildHost(args, encryptConstants);
var settings = host.Services.GetRequiredService<AppSettings>();
var config = host.Services.GetRequiredService<IReadOnlyDictionary<string, string>>();

var reader = host.Services.GetRequiredService<Reader>();
var writter = host.Services.GetRequiredService<Writter>();
var mapper = host.Services.GetRequiredService<Mapper>();
var parser = host.Services.GetRequiredService<Parser>();
var scrapper = host.Services.GetRequiredService<Scrapper>();

await scrapper.CheckPlayWrightInstallation();
await scrapper.RunAsync();

string imputPath = Path.Combine(settings.Folders.Data, settings.Folders.InPut);
string outputPath = Path.Combine(settings.Folders.Data, settings.Folders.OutPut);
string[] htmlFiles = reader.GetHtmlFilesList(imputPath);

if (htmlFiles.Length > 0)
{
    foreach (string htmlFile in htmlFiles)
    {
        string content = reader.GetHtml(htmlFile);
        (var parsedData, long parsedInMs) = await parser.ParseHtml(content);
        reader.Debug(Path.GetFileNameWithoutExtension(htmlFile), parsedData, parsedInMs);

        foreach (var entry in parsedData)
            mapper.Merge(entry);
    }
}

string fields = mapper.DrawFields([.. mapper._content.Values]);

string personalLog = mapper.MapToPersonalLog(settings.Generation.IgnoreGuilds);
writter.SaveAsLog(outputPath, personalLog);

JsonFile jsonOutput = mapper.MapToJson();
writter.SaveAsJson(outputPath, jsonOutput);

if (!settings.AutoClose)
{
    Console.WriteLine("Press Enter to exit...");
    Console.ReadLine();
}