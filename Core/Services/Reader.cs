using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.OutPut;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
namespace HtmlGamer.Core.Services;
public sealed class Reader
{
    private readonly ILogger<Reader> _logger;
    internal Reader()
    : this(NullLogger<Reader>.Instance)
    {
    }
    public Reader(ILogger<Reader> logger)
    {
        _logger = logger;

        Loggers.LogAs.Init(_logger);
    }
    internal JsonFile GetLogFromJson(string path)
    {
        string fileContent = string.Empty;

        if (!File.Exists(path))
            return new JsonFile();

        try
        {
            fileContent = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, path, ex);
            return new JsonFile();
        }

        if (string.IsNullOrEmpty(fileContent))
            return new JsonFile();

        try
        {
            return JsonSerializer.Deserialize<JsonFile>(fileContent, Constants.Json.SerializerOptions) ?? new JsonFile();
        }
        catch (JsonException ex)
        {
            Loggers.LogAs.Error(_logger, path, ex);
            return new JsonFile();
        }
    }
    internal List<JsonFile> GetLogList(string path)
    {
        List<JsonFile> list = [];

        foreach (var file in Directory.GetFiles(path, "*.json"))
        {
            var log = GetLogFromJson(file);
            if (log.Content != null && log.Content.Count > 0)
                list.Add(log);
        }

        return list;
    }
    internal string GetHtml(string path)
    {
        try
        {
            Loggers.LogAs.Debug(_logger, $"GetHtml: {path}");
            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, path, ex);
            return string.Empty;
        }
    }
    internal string GetJson(string path, string fileName)
    {
        Loggers.LogAs.Debug(_logger, $"GetJson: {fileName}, Path: {path}");
        fileName = Constants.GetFileName(fileName, "json");
        return GetFile(path, fileName);
    }
    internal string GetFile(string path, string fileName)
    {
        try
        {
            path = Path.Combine(path, fileName);
            Loggers.LogAs.Debug(_logger, $"GetFile: {fileName}");
            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, $"GetFile: {path}", ex);
            return string.Empty;
        }
    }
    internal string[] GetHtmlFilesList(string path) => Directory.Exists(path) ? Directory.GetFiles(path, "*.html") : new string[0];
    internal void Debug(string fileName, List<MemberGuildType> parsedData, long parsedIn)
    {
        Loggers.LogAs.Debug(_logger, $" --- {fileName} - {parsedData.Count} - {parsedIn}ms --- ");

        foreach (var data in parsedData)
        {
            string guild = string.Empty;
            if (!string.IsNullOrEmpty(data.Guild?.Name))
            {
                string title = string.IsNullOrEmpty(data.Member.Title) ? string.Empty : $" - {data.Member.Title}";
                guild = $" [ {data.Guild.Name}{title} ]";
            }

            int? id = default;
            if (data.Type == Data.Enums.Type.Member)
                id = data.Member.Id;
            else if (data.Type == Data.Enums.Type.Guild)
                id = data.Guild?.Id;

            Loggers.LogAs.Debug(_logger, $"{id} {data.Member.Name} {guild}");
        }
    }
}
