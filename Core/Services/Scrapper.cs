using HtmlGamer.Core.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
namespace HtmlGamer.Core.Services;
public sealed class Scrapper
{
    private readonly Random _random = new();
    
    private readonly ILogger<Scrapper> _logger;
    private readonly Writter _writter;
    
    private readonly string _gameUrl;
    private readonly string _masterPageSelector;

    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    public Scrapper(ILogger<Scrapper> logger, Writter writter, IReadOnlyDictionary<string,string> config)
    {
        _logger = logger;
        _writter = writter;
        _gameUrl = config[Constants.EncryptKeys.MasterUrl];
        _masterPageSelector = config[Constants.EncryptKeys.MasterPageSelector];

        _writter.CreateDirectory(Constants.Folders.OutputData);
        Loggers.LogAs.Init(_logger);
    }
    public async Task RunAsync()
    {
        using var playwright = await Playwright.CreateAsync();

        _browser = await playwright.Chromium.LaunchAsync(Constants.Browsers.LaunchOptions);
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();

        Loggers.LogAs.Debug(_logger, $"Opening Chromium... url: {_gameUrl}");
        await _page.GotoAsync(_gameUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // FILL FORM STEP
        //Console.WriteLine("Click Login button, then press ENTER ...");
        //await _page.FillAsync("input[name='username']", "yyourUser");

        // MANUAL LOGIN STEP
        Console.WriteLine("Login manually, then press ENTER ...");
        Console.ReadLine();

        var html = await _page.ContentAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await ClickButton("Attack");

        int pageIndex = 1;
        await GoToPage(pageIndex);

        string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        while (true)
        {        
            Loggers.LogAs.Debug(_logger, $"Processing page: {pageIndex} Url: {_page.Url}");
                             
            await WaitRandom();

            await SaveCurrentPageHtml(timeStamp, pageIndex);

            bool moved = await MoveToNextPage();

            if (!moved)
            {
                Loggers.LogAs.Debug(_logger, "No more pages.");
                break;
            }

            pageIndex++;

            await WaitRandom(3000, 7000);
        }

        Loggers.LogAs.Debug(_logger, "Done.");
    }
    private async Task ClickButton(string name)
    {
        if (_page == null)
            return;

        Loggers.LogAs.Debug(_logger, $"Clicking button: {name}...");

        name = name.ToLower();

        string? href = await _page
        .Locator($"area[alt='{name}']")
        .GetAttributeAsync("href");

        await _page.GotoAsync(new Uri(new Uri(_page.Url), href).ToString());
    }
    public async Task GoToPage(int pageId)
    {
        if (_page == null)
            return;

        await _page.FillAsync("input[name='page']", pageId.ToString());
        await _page.Locator(_masterPageSelector).EvaluateAsync("f => f.submit()");

        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Loggers.LogAs.Debug(_logger, $"Navigated to page {pageId} Url: {_page.Url}");
    }
    public async Task CheckPlayWrightInstallation()
    {
        try
        {
            using var playwright = await Playwright.CreateAsync();
            await playwright.Chromium.LaunchAsync();
        }
        catch (PlaywrightException ex)
            when (ex.Message.Contains("Executable doesn't exist"))
        {
            Loggers.LogAs.Error(_logger, "Playwright is not properly installed. Installing now...", ex);
            Program.Main(new[] { "install", "chromium" });
        }
        catch (Exception ex)      
        {
            Loggers.LogAs.Error(_logger, "Playwright is not properly installed. Installing now...", ex);
            Program.Main(new[] { "install", "chromium" });
        }
    }
    internal async Task SaveCurrentPageHtml(string timeStamp, int pageIndex)
    {
        if (_page == null)
            return;

        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var html = await _page.ContentAsync();

        var path = $"{Constants.Folders.OutputData}{timeStamp}";
        await _writter.SaveAsHtml(path, $"{pageIndex}_battlefield", html);

        Loggers.LogAs.Debug(_logger, $"Saved page with index: {pageIndex} Path: {path}");
    }
    internal async Task<bool> MoveToNextPage()
    {
        if (_page == null)
            return false;

        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await _page.Locator("a")
            .Filter(new() { HasText = "Next" })
            .Last
            .ClickAsync();

        Loggers.LogAs.Debug(_logger, "Moving to next page...");
        return true;
    }
    internal async Task WaitRandom(int minMs = 1200, int maxMs = 4500)
    {
        int delay = _random.Next(minMs, maxMs);

        Loggers.LogAs.Debug(_logger, $"Waiting for {delay}ms");

        await Task.Delay(delay);
    }
}
