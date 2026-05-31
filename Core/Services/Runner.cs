using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.OutPut;
using Microsoft.Extensions.Logging;
namespace HtmlGamer.Core.Services;

public sealed class Runner
{
    public bool AutoClose => _settings.AutoClose;
    private readonly Scrapper _scrapper;
    private readonly Parser _parser;
    private readonly Mapper _mapper;
    private readonly Writer _writer;
    private readonly Reader _reader;
    private readonly ILogger<Runner> _logger;
    private readonly AppSettings _settings;
    private ParsedData _data = new();
    public Runner(AppSettings settings, Scrapper scrapper, Parser parser, Mapper mapper, Writer writer, Reader reader, ILogger<Runner> logger)
    {
        _scrapper = scrapper;
        _parser = parser;
        _mapper = mapper;
        _writer = writer;
        _reader = reader;
        _logger = logger;
        _settings = settings;

        Loggers.LogAs.Init(_logger);
    }
    public async Task Scrapper()
    {
        if (_settings.Execute.Scrapper)
            await _scrapper.RunAsync();
    }
    public async Task Parser()
    {
        if (_settings.Execute.Parser)
        {
            string[] htmlFiles = _reader.GetHtmlFilesList(Path.Combine(_settings.Folders.Data, _settings.Folders.ToParse));

            if (htmlFiles.Length > 0)
            {
                foreach (string htmlFile in htmlFiles)
                {
                    string content = _reader.GetHtml(htmlFile);
                    (var parsedData, long parsedInMs) = await _parser.ParseHtml(content);
                    _reader.Debug(Path.GetFileNameWithoutExtension(htmlFile), parsedData, parsedInMs);

                    foreach (var entry in parsedData)
                        _parser.Merge(entry);
                }
            }

            _data = _parser.GetParsedData();
        }
    }
    public async Task Analayzer()
    {
        if (_settings.Execute.Parser)
        {
            string path = Path.Combine(_settings.Folders.Data, _settings.Folders.Analyzed);
            string fields = _mapper.DrawFields([.. _data.MebersGuilds.Values]);

            string personalLog = _mapper.MapToPersonalLog(_data, _settings.Generation.IgnoreGuilds);
            _writer.SaveAsLog(path, personalLog);

            JsonFile jsonOutput = _mapper.MapToJson(_data);
            _writer.SaveAsJson(path, jsonOutput);
        }
    }
    public async Task Run()
    {
        await Scrapper();
        await Parser();
        await Analayzer();
    }
    public void Close()
    {
        if (!_settings.AutoClose)
        {
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
