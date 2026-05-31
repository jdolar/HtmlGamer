using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models.OutPut;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
namespace HtmlGamer.Core.Services;
public sealed class Writer
{
    private readonly ILogger<Writer> _logger;
    internal Writer()
    : this(NullLogger<Writer>.Instance)
    {
    }
    public Writer(ILogger<Writer> logger)
    {
        _logger = logger;
        Loggers.LogAs.Init(_logger);
    }
    internal void SaveAsJson(string path, JsonFile content)
    {
        string fileName = Constants.GetFileName(extension: "json");
        string fileContent = JsonSerializer.Serialize<JsonFile>(content, Constants.Json.SerializerOptions);
        SaveFile(path, fileName, fileContent);
    }
    internal void SaveAsLog(string path, string content)
    {
        string fileName = Constants.GetFileName(extension: "log");
        SaveFile(path, fileName, content);
    }
    internal async Task SaveAsHtml(string path, string fileNamePrefix, string content)
    {
       string name = $"{fileNamePrefix}_{Constants.GetFileName(extension:"html")}";
       await SaveFileAsync(path, name, content);
    }
    internal void CreateDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path) == false)
            {
                Loggers.LogAs.Debug(_logger, $"Directory does not exist, creating it: {path}");
                Directory.CreateDirectory(path);
            }
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, $"Error occurred while creating directory: {path}", ex);
        }
    }
    internal void SaveAsDictionary(string path, string name, Dictionary<string, string> data)
    {
        string content = JsonSerializer.Serialize(data);
        SaveFile(path, name, content);
    }
    private void SaveFile(string path, string name, string content)
    {
        CreateDirectory(path);

        try
        {
            Loggers.LogAs.Debug(_logger, $"Saving: {name}");
            File.WriteAllText(Path.Combine(path, name), content);
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, name, ex);
        }
    }
    internal async Task SaveFileAsync(string path, string name, string content)
    {
        CreateDirectory(path);

        try
        {
            Loggers.LogAs.Debug(_logger, $"Saving: {name}, Path: {path}");
            File.WriteAllText(Path.Combine(path, name), content);
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, name, ex);
        }
    }
}

