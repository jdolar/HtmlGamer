using HtmlGamer.Core.Data.Enums;
using HtmlGamer.Core.Data.Interfaces;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
using HtmlGamer.Core.Data.Models.OutPut;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
namespace HtmlGamer.Core.Services;
public sealed class Runner
{
    public bool AutoClose => _settings.AutoClose;
    private readonly Scrapper _cruiser;
    private readonly Parser _parser;
    private readonly Mapper _mapper;
    private readonly Writer _writer;
    private readonly Reader _reader;
    private readonly IVpnClient _vpn;
    private readonly ILogger<Runner> _logger;
    private readonly AppSettings _settings;
    private readonly Dictionary<Step, Func<IPage, Account, Task>> _steps;
    private readonly IReadOnlyList<Account> _accounts;
    private readonly IReadOnlyList<Scenario> _scenarios;
    private readonly IReadOnlyList<Execute> _executes;
    private ParsedData _data = new();
    public Runner(AppSettings settings, Scrapper scrapper, Parser parser, Mapper mapper, Writer writer, Reader reader, IReadOnlyList<Account> accounts, IReadOnlyList<Data.Models.InPut.Scenario> scenarios, IReadOnlyList<Execute> execute, IVpnClient vpn, ILogger<Runner> logger)
    {
        _cruiser = scrapper;
        _parser = parser;
        _mapper = mapper;
        _writer = writer;
        _reader = reader;
        _vpn = vpn;
        _logger = logger;
        _accounts = accounts;
        _scenarios = scenarios;
        _executes = execute;
        _settings = settings;
        _steps = new()
        {
            { Step.Login, Login },
            { Step.Bank, Bank },
            { Step.ScrapBattleField, ScrapBattlefield },        
            { Step.Parse, (_, account) => Parser(account) },
            { Step.Analyze, (_, account) => Analyzer(account) }
        };
        Loggers.LogAs.Init(_logger);
    }
    public async Task RunScenarios()
    {
        foreach (var execute in _executes)
        {
            Loggers.LogAs.Debug(_logger, "Starting scenario: ", execute.ScenarioName);
            await RunScenario(execute.UserName, execute.ScenarioName);
            Loggers.LogAs.Debug(_logger, "Completed scenario: ", execute.ScenarioName);
        }  
    }
    public async Task RunScenario(string userName, string scenarioName)
    {
        Scenario? scenario = _scenarios.FirstOrDefault(s => s.Name.Equals(scenarioName, StringComparison.OrdinalIgnoreCase));
        Account? account = _accounts.FirstOrDefault(a => a.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

        await _vpn.ClearTunnels(CancellationToken.None);

        if (account == null || scenario == null)
        {
            Loggers.LogAs.Error(_logger, "Account or scenario not found.", "");
            return;
        }

        bool useVpn = !string.IsNullOrEmpty(account.Vpn);
        string vpnType = useVpn ? account.Vpn : "No VPN";
        string vnpName = $"{account.Vpn} [{vpnType}]";
        if (useVpn)
        {
            Loggers.LogAs.Debug(_logger, "Connecting  VPN: ", vnpName);
            await _vpn.ConnectAsync(account.Vpn);
        }

        await _cruiser.Execute(async page =>
        {
            foreach (var step in scenario.Steps)
            {
                if (_steps.TryGetValue(step, out var action))
                {
                    string stepName = step.ToString();
                    Loggers.Scenario.Invoke(_logger, stepName);

                    await action(page, account);

                    Loggers.Scenario.Complete(_logger, stepName);
                }
            }
        });

        if (useVpn)
        {
            Loggers.LogAs.Debug(_logger, "Disconnecting VPN: ", vnpName);
            await _vpn.DisconnectAsync(account.Vpn);
        }

        await _cruiser.RandomDelay();
    }
    public async Task Parser(Account _)
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
    public async Task Analyzer(Account _)
    {
            string path = Path.Combine(_settings.Folders.Data, _settings.Folders.Analyzed);
            string fields = _mapper.DrawFields([.. _data.MebersGuilds.Values]);

            string personalLog = _mapper.MapToPersonalLog(_data, _settings.Generation.IgnoreGuilds);
            _writer.SaveAsLog(path, personalLog);

            JsonFile jsonOutput = _mapper.MapToJson(_data);
            _writer.SaveAsJson(path, jsonOutput);
    }
    public async Task Bank(IPage page, Account account) => await  _cruiser.Bank(page, account);
    public async Task Login(IPage page, Account account) => await _cruiser.Login(page, account);
    public async Task ScrapBattlefield(IPage page, Account account) => await _cruiser.ScrapBattlefield(page,account);
    public void Close()
    {
        if (!_settings.AutoClose)
        {
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
